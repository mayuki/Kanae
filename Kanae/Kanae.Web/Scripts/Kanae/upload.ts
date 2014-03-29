module Kanae.Upload {
    export class Application {
        static Current = new Application();

        requestValidationToken = '';
        postUrl = '';

        viewModel = {
            progresses: ko.observableArray<UploadProgressItem>()
        };

        constructor() {
            this.start();
        }

        start(): void {
            if (!this.isBrowserSupport()) {
                return;
            }

            document.addEventListener('DOMContentLoaded', () => this.onPageReady());
        }

        onPageReady(): void {
            var formE = <HTMLFormElement>document.getElementById('upload-form');
            var inputFileE = <HTMLInputElement>formE.querySelector('input[type="file"]');
            inputFileE.multiple = true;

            this.postUrl = formE.action;
            this.requestValidationToken = (<HTMLInputElement>formE.querySelector('input[name="__RequestVerificationToken"]')).value;

            formE.querySelector('input[type="submit"]').addEventListener('click', (e) => {
                e.preventDefault();
                for (var i = 0; i < inputFileE.files.length; i++) {
                    this.upload(inputFileE.files[i]);
                }
                formE.reset();
            });
            document.addEventListener('dragenter', (e) => e.preventDefault());
            document.addEventListener('dragover', (e) => e.preventDefault());
            document.addEventListener('dragleave', (e) => e.preventDefault());
            document.addEventListener('drop', (e) => {
                e.preventDefault();
                for (var i = 0; i < e.dataTransfer.files.length; i++) {
                    this.upload(e.dataTransfer.files[i]);
                }
            });

            // ペースト
            document.body.addEventListener("paste", e => this.handlePaste(e));
            window.addEventListener("keydown", e => {
                if (document.queryCommandSupported('paste') && e.keyCode == 86 && e.ctrlKey) {
                    document.execCommand('paste');
                }
            });

            ko.applyBindings(this.viewModel, document.querySelector('#upload-progresses-container'));
        }

        handlePaste(e: Event): void {
            e.preventDefault();
            var clipboardData = window.clipboardData || (<any>e).clipboardData;
            var itemsOrFiles = clipboardData.items || clipboardData.files;
            for (var i = 0; i < itemsOrFiles.length; i++) {
                var fileOrItem = itemsOrFiles[i];
                if (fileOrItem.type.match(/^image\//)) {
                    var file = fileOrItem.getAsFile ? fileOrItem.getAsFile() : fileOrItem;
                    setTimeout(() => this.upload(file), 1);
                }
            }
        }

        upload(file: File) {
            var progressItem = new UploadProgressItem(file.name || 'Untitled');
            this.viewModel.progresses.unshift(progressItem);

            var xhr = new XMLHttpRequest();
            var formData = new FormData();

            xhr.addEventListener('progress', (e: ProgressEvent) => {
                //console.log('%d/%d (%d)', e.loaded, e.total, e.loaded / e.total);
                if (e.lengthComputable) {
                    progressItem.percent((e.loaded / e.total) * 100);
                } else {
                    progressItem.percent(null);
                }
            });
            xhr.addEventListener('load', (e) => {
                progressItem.percent(100);
                progressItem.imageUrl(JSON.parse(xhr.responseText).Url);
                progressItem.editUrl(JSON.parse(xhr.responseText).EditUrl);
            });

            formData.append('withXhr', 'true');
            formData.append('uploadedFile', file);
            formData.append('__RequestVerificationToken', this.requestValidationToken);

            xhr.open('POST', this.postUrl, true);
            xhr.send(formData);
        }

        isBrowserSupport(): boolean {
            return ((<any>window).FormData);
        }
    }

    export class UploadProgressItem {
        percent: KnockoutObservable<number>;
        name: KnockoutObservable<string>;
        imageUrl: KnockoutObservable<string>;
        editUrl: KnockoutObservable<string>;

        constructor(name: string) {
            this.percent = ko.observable(null);
            this.name = ko.observable(name);
            this.imageUrl = ko.observable('');
            this.editUrl = ko.observable('');
        }
    }
}