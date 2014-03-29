/// <referneces path="../_references.ts" />
module Kanae {
    export class Utility {
        /**
         * 画像の読み込みを待つPromiseを返します。
         */
        static waitForImageLoaded(imageE: HTMLImageElement): Q.Promise<HTMLImageElement> {
            var d = Q.defer<HTMLImageElement>();

            if (imageE.naturalHeight != 0) {
                d.resolve(imageE);
            } else {
                imageE.onload = () => d.resolve(imageE);
                imageE.onerror = () => d.reject(imageE);
            }

            return d.promise;
        }
    }
}

module Kanae.Drawing {
    export class Application {
        static Current = new Application();

        drawingToolbar: HTMLElement;
        baseImage: HTMLImageElement;
        targetCanvas: HTMLCanvasElement;
        bufferedCanvas: HTMLCanvasElement;

        canvasSelectionHandler: CanvasSelectionHandler;
        canvasHandler: CanvasHandler;
        jsColorInstance: any;

        constructor() {
            this.start();
        }

        start(): void {
            document.addEventListener('DOMContentLoaded', () => this.onPageReady());
        }

        onPageReady(): void {
            if (!this.isBrowserSupport()) {
                return;
            }

            document.body.classList.add('kanae-drawing-supported');

            this.baseImage = <HTMLImageElement>document.querySelector('#drawing-base-image');
            this.targetCanvas = <HTMLCanvasElement>document.querySelector('#drawing-canvas');
            this.bufferedCanvas = <HTMLCanvasElement>document.querySelector('#drawing-canvas-buffer');

            Kanae.Utility.waitForImageLoaded(this.baseImage).then(() => {
                document.body.classList.add('kanae-drawing-ready');
                this.onLoadCompleted();
            });
        }

        onLoadCompleted(): void {
            // いろいろ準備する

            // Canvasの準備
            this.targetCanvas.width = this.baseImage.naturalWidth;
            this.targetCanvas.height = this.baseImage.naturalHeight;
            this.canvasHandler = new CanvasHandler(this.targetCanvas, this.bufferedCanvas);
            this.canvasHandler.setup();
            this.canvasSelectionHandler = new CanvasSelectionHandler(this.bufferedCanvas);
            this.canvasSelectionHandler.setup();

            // カラーピッカー
            this.jsColorInstance = new (<any>window).jscolor.color(document.getElementById('drawing-toolbar-color'));
            this.jsColorInstance.onImmediateChange = () => {
                this.canvasHandler.color = '#' + this.jsColorInstance.toString();
            };
            this.canvasHandler.color = '#' + this.jsColorInstance.toString();
            this.bufferedCanvas.addEventListener('pointerdown', (e: MSPointerEvent) => {
                this.jsColorInstance.valueElement.blur();
                this.jsColorInstance.hidePicker();
            });

            // あとでKnockoutに…
            // ツールバー
            this.drawingToolbar = (<HTMLElement>document.querySelector('#drawing-toolbar'));
            document.querySelector('#drawing-toolbar-save').addEventListener('click', (e) => {
                e.preventDefault();

                var dataUrl = this.targetCanvas.toDataURL('image/png');
                var antiForgeryToken = (<HTMLInputElement>document.querySelector('[name=__RequestVerificationToken]')).value;

                var xhr = new XMLHttpRequest();
                xhr.open('POST', window.location.href, true);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                xhr.onload = () => {
                    if (xhr.status >= 200 && xhr.status <= 299) {
                        var result = JSON.parse(xhr.responseText);
                        if (result.Success) {
                            window.location.pathname = result.Url;
                        } else {
                            alert('アップロード時にエラーが発生しました: ' + result.ErrorMessage);
                        }
                    } else {
                        alert('アップロード時にエラーが発生しました: ' + xhr.status + ' (' + xhr.statusText + ')');
                    }
                };
                xhr.send(
                    [
                        ['uploadData', dataUrl],
                        ['__RequestVerificationToken', antiForgeryToken],
                        ['isOverwrite', 'true']
                    ].map(v => encodeURIComponent(v[0]) + '=' + encodeURIComponent(v[1])).join('&')
                );
            });
            document.querySelector('#drawing-toolbar-strokesize').addEventListener('change', (e) => {
                this.canvasHandler.lineWidthBase = parseInt((<HTMLSelectElement>e.target).value);
                this.canvasHandler.lineWidthMin = parseInt((<HTMLSelectElement>e.target).value) / 3;
            });
            document.querySelector('#drawing-toolbar-opacity').addEventListener('change', (e) => {
                this.canvasHandler.opacity = parseInt((<HTMLInputElement>e.target).value);
            });
            this.canvasHandler.opacity = parseInt((<HTMLInputElement>document.querySelector('#drawing-toolbar-opacity')).value);
            document.querySelector('#drawing-toolbar-clear').addEventListener('click', (e) => {
                this.canvasHandler.clear();
            });
            document.querySelector('#drawing-toolbar-crop').addEventListener('click', (e) => {
                if (this.canvasHandler.isDirty) {
                    if (confirm('切り抜きをする前に描き込みを保存する必要があります。\n続行すると作業中の状態は失われますがよろしいですか?')) {
                        this.canvasHandler.clear();
                    } else {
                        return;
                    }
                }

                this.drawingToolbar.classList.add('drawing-toolbar-ModeCropping');
                this.canvasSelectionHandler.start();
                this.canvasHandler.isEnabled = false;
                this.canvasSelectionHandler.isEnabled = true;
            });
            document.querySelector('#drawing-toolbar-crop-cancel').addEventListener('click', (e) => {
                this.drawingToolbar.classList.remove('drawing-toolbar-ModeCropping');
                this.canvasSelectionHandler.cancel();
                this.canvasHandler.isEnabled = true;
                this.canvasSelectionHandler.isEnabled = false;
            });
            document.querySelector('#drawing-toolbar-crop-commit').addEventListener('click', (e) => {
                e.preventDefault();

                if (this.canvasSelectionHandler.selection.width == 0 || this.canvasSelectionHandler.selection.height == 0) {
                    alert('切り取る領域が選択されていません。');
                    return;
                }

                var formE = <HTMLFormElement>document.querySelector('#drawing-crop-form');
                (<HTMLInputElement>formE.querySelector('[name="x"]')).value = this.canvasSelectionHandler.selection.x.toString();
                (<HTMLInputElement>formE.querySelector('[name="y"]')).value = this.canvasSelectionHandler.selection.y.toString();
                (<HTMLInputElement>formE.querySelector('[name="width"]')).value = this.canvasSelectionHandler.selection.width.toString();
                (<HTMLInputElement>formE.querySelector('[name="height"]')).value = this.canvasSelectionHandler.selection.height.toString();
                formE.submit();
            });
        }

        isBrowserSupport(): boolean {
            return ('getContext' in document.createElement('canvas'));
        }
    }

    export class CanvasSelectionHandler {
        canvasE: HTMLCanvasElement;
        ctx: CanvasRenderingContext2D;

        isPointerDown: boolean;
        isEnabled = false;
        prevPoint: Point;

        selection = { x: 0, y: 0, width: 0, height: 0 };

        constructor(canvasE: HTMLCanvasElement) {
            this.canvasE = canvasE;
            this.ctx = this.canvasE.getContext('2d');
        }

        // TODO: MSPointerEvent を使うのは TypeScript でPointerEventsの定義がまだないから…。
        setup(): void {
            this.canvasE.addEventListener('pointermove', (e: MSPointerEvent) => this.onPointerMove(e));
            this.canvasE.addEventListener('pointerdown', (e: MSPointerEvent) => this.onPointerDown(e));
            this.canvasE.addEventListener('pointerup', (e: MSPointerEvent) => this.onPointerUp(e));
            this.canvasE.addEventListener('pointerout', (e: MSPointerEvent) => this.onPointerOut(e));
        }

        start(): void {
            this.canvasE.style.cursor = 'crosshair';
        }

        cancel(): void {
            this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
            this.canvasE.style.cursor = 'default';
        }

        onPointerDown(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isEnabled) return;

            this.paintStart(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            this.isPointerDown = true;
        }
        onPointerUp(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.isPointerDown = false;
            this.paintEnd();
        }
        onPointerMove(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
        }
        onPointerOut(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            this.paintEnd();

            this.isPointerDown = false;
        }

        paintStart(x: number, y: number, p: number): void {
            this.canvasE.style.opacity = '1';
            this.prevPoint = new Point(x, y, p);
            // 線のスタイルを初期化
            this.ctx.strokeStyle = '#000';
            this.ctx.lineWidth   = 1;
            this.ctx.lineCap     = 'round';
            this.ctx.lineJoin    = 'round';
        }

        paintMove(x: number, y: number, p: number): void {
            this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
            this.ctx.fillStyle = 'rgba(0, 0, 0, 0.75)';
            this.ctx.fillRect(0, 0, this.canvasE.width, this.canvasE.height);
            this.ctx.clearRect(this.prevPoint.x, this.prevPoint.y, x - this.prevPoint.x, y - this.prevPoint.y);

            this.selection.x = this.prevPoint.x;
            this.selection.y = this.prevPoint.y;
            this.selection.width  = x - this.prevPoint.x;
            this.selection.height = y - this.prevPoint.y;
        }

        paintEnd(): void {
            this.canvasE.style.cursor = 'default';

            if (this.selection.width < 0) {
                this.selection.width *= -1;
                this.selection.x -= this.selection.width;
            }
            if (this.selection.height < 0) {
                this.selection.height *= -1;
                this.selection.y -= this.selection.height;
            }
        }
    }

    export class CanvasHandler {
        canvasE: HTMLCanvasElement;
        bufferedCanvasE: HTMLCanvasElement;

        ctx: CanvasRenderingContext2D;
        bufferedCtx: CanvasRenderingContext2D;

        isPointerDown: boolean;
        pointBuffer: Point[] = [];
        prevPoint: Point;

        lineWidthMin  = 2;
        lineWidthBase = 10;
        opacity       = 100;
        color         = '#ff0000';
        isDirty       = false;
        isEnabled     = true;

        constructor(canvasE: HTMLCanvasElement, bufferedCanvasE: HTMLCanvasElement) {
            this.canvasE = canvasE;
            this.bufferedCanvasE = bufferedCanvasE;

            this.bufferedCanvasE.width = this.canvasE.width;
            this.bufferedCanvasE.height = this.canvasE.height;

            this.ctx = this.canvasE.getContext('2d');
            this.bufferedCtx = this.bufferedCanvasE.getContext('2d');
        }

        // TODO: MSPointerEvent を使うのは TypeScript でPointerEventsの定義がまだないから…。
        setup(): void {
            this.bufferedCanvasE.addEventListener('pointermove', (e: MSPointerEvent) => this.onPointerMove(e));
            this.bufferedCanvasE.addEventListener('pointerdown', (e: MSPointerEvent) => this.onPointerDown(e));
            this.bufferedCanvasE.addEventListener('pointerup'  , (e: MSPointerEvent) => this.onPointerUp(e));
            this.bufferedCanvasE.addEventListener('pointerout' , (e: MSPointerEvent) => this.onPointerOut(e));
        }

        onPointerDown(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isEnabled) return;

            this.paintStart(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            this.isPointerDown = true;
        }
        onPointerUp(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.isPointerDown = false;
            this.paintEnd();
        }
        onPointerMove(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
        }
        onPointerOut(e: MSPointerEvent): void {
            e.preventDefault();
            if (!this.isPointerDown || !this.isEnabled) return;

            this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            this.paintEnd();

            this.isPointerDown = false;
        }

        clear(): void {
            this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
            this.isDirty = false;
        }

        paintStart(x: number, y: number, p: number): void {
            this.pointBuffer = [];
            this.prevPoint = null;
            this.isDirty = true;

            // 線のスタイルを初期化
            this.bufferedCtx.strokeStyle = this.color;
            this.bufferedCtx.lineWidth = this.lineWidthBase;
            this.bufferedCtx.lineCap = 'round';
            this.bufferedCtx.lineJoin = 'round';

            this.bufferedCanvasE.style.opacity = (this.opacity / 100.0).toString();

            this.paintMove(x, y, p);
        }

        paintMove(x: number, y: number, p: number): void {
            var point = this.getPoint(x, y, p);

            // lineTo使ってるので同じ位置だと点が打てない
            if (this.prevPoint == null) {
                this.prevPoint = new Point(point.x + 0.0001, point.y + 0.0001, point.p);
            }

            this.bufferedCtx.lineWidth = this.lineWidthMin + (this.lineWidthBase * point.p - 0.5);

            this.bufferedCtx.beginPath();
            this.bufferedCtx.moveTo(this.prevPoint.x, this.prevPoint.y);
            this.bufferedCtx.lineTo(point.x, point.y);
            this.bufferedCtx.stroke();

            this.prevPoint = point;
        }

        paintEnd(): void {
            // 最後に合体。合体するときに透明度がきく。
            this.ctx.globalAlpha = (this.opacity / 100.0);
            this.ctx.drawImage(this.bufferedCanvasE, 0, 0);
            this.bufferedCtx.clearRect(0, 0, this.bufferedCanvasE.width, this.bufferedCanvasE.height);
        }

        /**
         * バッファに詰めて最近の平均のポイントを返します
         */
        getPoint(x: number, y: number, p: number) {
            this.pointBuffer.push({ x: x, y: y, p: p });

            if (this.pointBuffer.length > 5) {
                this.pointBuffer.shift();
            }
            var avgPoint = this.pointBuffer.reduce((r, v, index) => {
                r.x += v.x;
                r.y += v.y;
                r.p += v.p;
                if (index == this.pointBuffer.length - 1) {
                    r.x /= index + 1;
                    r.y /= index + 1;
                    r.p /= index + 1;
                }
                return r;
            }, { x: 0, y: 0, p: 0 });

            return avgPoint;
        }
    }

    export class Point {
        x: number;
        y: number;
        p: number;

        constructor(x: number, y: number, p: number = 0) {
            this.x = x;
            this.y = y;
            this.p = p;
        }
    }
}

