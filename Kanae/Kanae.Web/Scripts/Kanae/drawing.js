/// <referneces path="../_references.ts" />
var Kanae;
(function (Kanae) {
    var Utility = (function () {
        function Utility() {
        }
        /**
        * 画像の読み込みを待つPromiseを返します。
        */
        Utility.waitForImageLoaded = function (imageE) {
            var d = Q.defer();

            if (imageE.naturalHeight != 0) {
                d.resolve(imageE);
            } else {
                imageE.onload = function () {
                    return d.resolve(imageE);
                };
                imageE.onerror = function () {
                    return d.reject(imageE);
                };
            }

            return d.promise;
        };
        return Utility;
    })();
    Kanae.Utility = Utility;
})(Kanae || (Kanae = {}));

var Kanae;
(function (Kanae) {
    (function (Drawing) {
        var Application = (function () {
            function Application() {
                this.start();
            }
            Application.prototype.start = function () {
                var _this = this;
                document.addEventListener('DOMContentLoaded', function () {
                    return _this.onPageReady();
                });
            };

            Application.prototype.onPageReady = function () {
                var _this = this;
                if (!this.isBrowserSupport()) {
                    return;
                }

                document.body.classList.add('kanae-drawing-supported');

                this.baseImage = document.querySelector('#drawing-base-image');
                this.targetCanvas = document.querySelector('#drawing-canvas');
                this.bufferedCanvas = document.querySelector('#drawing-canvas-buffer');

                Kanae.Utility.waitForImageLoaded(this.baseImage).then(function () {
                    document.body.classList.add('kanae-drawing-ready');
                    _this.onLoadCompleted();
                });
            };

            Application.prototype.onLoadCompleted = function () {
                var _this = this;
                // いろいろ準備する
                // Canvasの準備
                this.targetCanvas.width = this.baseImage.naturalWidth;
                this.targetCanvas.height = this.baseImage.naturalHeight;
                this.canvasHandler = new CanvasHandler(this.targetCanvas, this.bufferedCanvas);
                this.canvasHandler.setup();
                this.canvasSelectionHandler = new CanvasSelectionHandler(this.bufferedCanvas);
                this.canvasSelectionHandler.setup();

                // カラーピッカー
                this.jsColorInstance = new window.jscolor.color(document.getElementById('drawing-toolbar-color'));
                this.jsColorInstance.onImmediateChange = function () {
                    _this.canvasHandler.color = '#' + _this.jsColorInstance.toString();
                };
                this.canvasHandler.color = '#' + this.jsColorInstance.toString();
                this.bufferedCanvas.addEventListener('pointerdown', function (e) {
                    _this.jsColorInstance.valueElement.blur();
                    _this.jsColorInstance.hidePicker();
                });

                // あとでKnockoutに…
                // ツールバー
                this.drawingToolbar = document.querySelector('#drawing-toolbar');
                document.querySelector('#drawing-toolbar-save').addEventListener('click', function (e) {
                    e.preventDefault();

                    var dataUrl = _this.targetCanvas.toDataURL('image/png');
                    var antiForgeryToken = document.querySelector('[name=__RequestVerificationToken]').value;

                    var xhr = new XMLHttpRequest();
                    xhr.open('POST', window.location.href, true);
                    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                    xhr.onload = function () {
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
                    xhr.send([
                        ['uploadData', dataUrl],
                        ['__RequestVerificationToken', antiForgeryToken],
                        ['isOverwrite', 'true']
                    ].map(function (v) {
                        return encodeURIComponent(v[0]) + '=' + encodeURIComponent(v[1]);
                    }).join('&'));
                });
                document.querySelector('#drawing-toolbar-strokesize').addEventListener('change', function (e) {
                    _this.canvasHandler.lineWidthBase = parseInt(e.target.value);
                    _this.canvasHandler.lineWidthMin = parseInt(e.target.value) / 3;
                });
                document.querySelector('#drawing-toolbar-opacity').addEventListener('change', function (e) {
                    _this.canvasHandler.opacity = parseInt(e.target.value);
                });
                this.canvasHandler.opacity = parseInt(document.querySelector('#drawing-toolbar-opacity').value);
                document.querySelector('#drawing-toolbar-clear').addEventListener('click', function (e) {
                    _this.canvasHandler.clear();
                });
                document.querySelector('#drawing-toolbar-crop').addEventListener('click', function (e) {
                    if (_this.canvasHandler.isDirty) {
                        if (confirm('切り抜きをする前に描き込みを保存する必要があります。\n続行すると作業中の状態は失われますがよろしいですか?')) {
                            _this.canvasHandler.clear();
                        } else {
                            return;
                        }
                    }

                    _this.drawingToolbar.classList.add('drawing-toolbar-ModeCropping');
                    _this.canvasSelectionHandler.start();
                    _this.canvasHandler.isEnabled = false;
                    _this.canvasSelectionHandler.isEnabled = true;
                });
                document.querySelector('#drawing-toolbar-crop-cancel').addEventListener('click', function (e) {
                    _this.drawingToolbar.classList.remove('drawing-toolbar-ModeCropping');
                    _this.canvasSelectionHandler.cancel();
                    _this.canvasHandler.isEnabled = true;
                    _this.canvasSelectionHandler.isEnabled = false;
                });
                document.querySelector('#drawing-toolbar-crop-commit').addEventListener('click', function (e) {
                    e.preventDefault();

                    if (_this.canvasSelectionHandler.selection.width == 0 || _this.canvasSelectionHandler.selection.height == 0) {
                        alert('切り取る領域が選択されていません。');
                        return;
                    }

                    var formE = document.querySelector('#drawing-crop-form');
                    formE.querySelector('[name="x"]').value = _this.canvasSelectionHandler.selection.x.toString();
                    formE.querySelector('[name="y"]').value = _this.canvasSelectionHandler.selection.y.toString();
                    formE.querySelector('[name="width"]').value = _this.canvasSelectionHandler.selection.width.toString();
                    formE.querySelector('[name="height"]').value = _this.canvasSelectionHandler.selection.height.toString();
                    formE.submit();
                });
            };

            Application.prototype.isBrowserSupport = function () {
                return ('getContext' in document.createElement('canvas'));
            };
            Application.Current = new Application();
            return Application;
        })();
        Drawing.Application = Application;

        var CanvasSelectionHandler = (function () {
            function CanvasSelectionHandler(canvasE) {
                this.isEnabled = false;
                this.selection = { x: 0, y: 0, width: 0, height: 0 };
                this.canvasE = canvasE;
                this.ctx = this.canvasE.getContext('2d');
            }
            // TODO: MSPointerEvent を使うのは TypeScript でPointerEventsの定義がまだないから…。
            CanvasSelectionHandler.prototype.setup = function () {
                var _this = this;
                this.canvasE.addEventListener('pointermove', function (e) {
                    return _this.onPointerMove(e);
                });
                this.canvasE.addEventListener('pointerdown', function (e) {
                    return _this.onPointerDown(e);
                });
                this.canvasE.addEventListener('pointerup', function (e) {
                    return _this.onPointerUp(e);
                });
                this.canvasE.addEventListener('pointerout', function (e) {
                    return _this.onPointerOut(e);
                });
            };

            CanvasSelectionHandler.prototype.start = function () {
                this.canvasE.style.cursor = 'crosshair';
            };

            CanvasSelectionHandler.prototype.cancel = function () {
                this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
                this.canvasE.style.cursor = 'default';
            };

            CanvasSelectionHandler.prototype.onPointerDown = function (e) {
                e.preventDefault();
                if (!this.isEnabled)
                    return;

                this.paintStart(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
                this.isPointerDown = true;
            };
            CanvasSelectionHandler.prototype.onPointerUp = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.isPointerDown = false;
                this.paintEnd();
            };
            CanvasSelectionHandler.prototype.onPointerMove = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            };
            CanvasSelectionHandler.prototype.onPointerOut = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
                this.paintEnd();

                this.isPointerDown = false;
            };

            CanvasSelectionHandler.prototype.paintStart = function (x, y, p) {
                this.canvasE.style.opacity = '1';
                this.prevPoint = new Point(x, y, p);

                // 線のスタイルを初期化
                this.ctx.strokeStyle = '#000';
                this.ctx.lineWidth = 1;
                this.ctx.lineCap = 'round';
                this.ctx.lineJoin = 'round';
            };

            CanvasSelectionHandler.prototype.paintMove = function (x, y, p) {
                this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
                this.ctx.fillStyle = 'rgba(0, 0, 0, 0.75)';
                this.ctx.fillRect(0, 0, this.canvasE.width, this.canvasE.height);
                this.ctx.clearRect(this.prevPoint.x, this.prevPoint.y, x - this.prevPoint.x, y - this.prevPoint.y);

                this.selection.x = this.prevPoint.x;
                this.selection.y = this.prevPoint.y;
                this.selection.width = x - this.prevPoint.x;
                this.selection.height = y - this.prevPoint.y;
            };

            CanvasSelectionHandler.prototype.paintEnd = function () {
                this.canvasE.style.cursor = 'default';

                if (this.selection.width < 0) {
                    this.selection.width *= -1;
                    this.selection.x -= this.selection.width;
                }
                if (this.selection.height < 0) {
                    this.selection.height *= -1;
                    this.selection.y -= this.selection.height;
                }
            };
            return CanvasSelectionHandler;
        })();
        Drawing.CanvasSelectionHandler = CanvasSelectionHandler;

        var CanvasHandler = (function () {
            function CanvasHandler(canvasE, bufferedCanvasE) {
                this.pointBuffer = [];
                this.lineWidthMin = 2;
                this.lineWidthBase = 10;
                this.opacity = 100;
                this.color = '#ff0000';
                this.isDirty = false;
                this.isEnabled = true;
                this.canvasE = canvasE;
                this.bufferedCanvasE = bufferedCanvasE;

                this.bufferedCanvasE.width = this.canvasE.width;
                this.bufferedCanvasE.height = this.canvasE.height;

                this.ctx = this.canvasE.getContext('2d');
                this.bufferedCtx = this.bufferedCanvasE.getContext('2d');
            }
            // TODO: MSPointerEvent を使うのは TypeScript でPointerEventsの定義がまだないから…。
            CanvasHandler.prototype.setup = function () {
                var _this = this;
                this.bufferedCanvasE.addEventListener('pointermove', function (e) {
                    return _this.onPointerMove(e);
                });
                this.bufferedCanvasE.addEventListener('pointerdown', function (e) {
                    return _this.onPointerDown(e);
                });
                this.bufferedCanvasE.addEventListener('pointerup', function (e) {
                    return _this.onPointerUp(e);
                });
                this.bufferedCanvasE.addEventListener('pointerout', function (e) {
                    return _this.onPointerOut(e);
                });
            };

            CanvasHandler.prototype.onPointerDown = function (e) {
                e.preventDefault();
                if (!this.isEnabled)
                    return;

                this.paintStart(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
                this.isPointerDown = true;
            };
            CanvasHandler.prototype.onPointerUp = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.isPointerDown = false;
                this.paintEnd();
            };
            CanvasHandler.prototype.onPointerMove = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
            };
            CanvasHandler.prototype.onPointerOut = function (e) {
                e.preventDefault();
                if (!this.isPointerDown || !this.isEnabled)
                    return;

                this.paintMove(Math.max(0, e.offsetX), Math.max(0, e.offsetY), e.pressure);
                this.paintEnd();

                this.isPointerDown = false;
            };

            CanvasHandler.prototype.clear = function () {
                this.ctx.clearRect(0, 0, this.canvasE.width, this.canvasE.height);
                this.isDirty = false;
            };

            CanvasHandler.prototype.paintStart = function (x, y, p) {
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
            };

            CanvasHandler.prototype.paintMove = function (x, y, p) {
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
            };

            CanvasHandler.prototype.paintEnd = function () {
                // 最後に合体。合体するときに透明度がきく。
                this.ctx.globalAlpha = (this.opacity / 100.0);
                this.ctx.drawImage(this.bufferedCanvasE, 0, 0);
                this.bufferedCtx.clearRect(0, 0, this.bufferedCanvasE.width, this.bufferedCanvasE.height);
            };

            /**
            * バッファに詰めて最近の平均のポイントを返します
            */
            CanvasHandler.prototype.getPoint = function (x, y, p) {
                var _this = this;
                this.pointBuffer.push({ x: x, y: y, p: p });

                if (this.pointBuffer.length > 5) {
                    this.pointBuffer.shift();
                }
                var avgPoint = this.pointBuffer.reduce(function (r, v, index) {
                    r.x += v.x;
                    r.y += v.y;
                    r.p += v.p;
                    if (index == _this.pointBuffer.length - 1) {
                        r.x /= index + 1;
                        r.y /= index + 1;
                        r.p /= index + 1;
                    }
                    return r;
                }, { x: 0, y: 0, p: 0 });

                return avgPoint;
            };
            return CanvasHandler;
        })();
        Drawing.CanvasHandler = CanvasHandler;

        var Point = (function () {
            function Point(x, y, p) {
                if (typeof p === "undefined") { p = 0; }
                this.x = x;
                this.y = y;
                this.p = p;
            }
            return Point;
        })();
        Drawing.Point = Point;
    })(Kanae.Drawing || (Kanae.Drawing = {}));
    var Drawing = Kanae.Drawing;
})(Kanae || (Kanae = {}));
//# sourceMappingURL=drawing.js.map
