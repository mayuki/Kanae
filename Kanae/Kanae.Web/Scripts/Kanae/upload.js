var Kanae;
(function (Kanae) {
    var Upload;
    (function (Upload) {
        var Application = (function () {
            function Application() {
                this.requestValidationToken = '';
                this.postUrl = '';
                this.viewModel = {
                    progresses: ko.observableArray()
                };
                this.start();
            }
            Application.prototype.start = function () {
                var _this = this;
                if (!this.isBrowserSupport()) {
                    return;
                }
                document.addEventListener('DOMContentLoaded', function () { return _this.onPageReady(); });
            };
            Application.prototype.onPageReady = function () {
                var _this = this;
                var formE = document.getElementById('upload-form');
                var inputFileE = formE.querySelector('input[type="file"]');
                inputFileE.multiple = true;
                this.postUrl = formE.action;
                this.requestValidationToken = formE.querySelector('input[name="__RequestVerificationToken"]').value;
                formE.querySelector('input[type="submit"]').addEventListener('click', function (e) {
                    e.preventDefault();
                    for (var i = 0; i < inputFileE.files.length; i++) {
                        _this.upload(inputFileE.files[i]);
                    }
                    formE.reset();
                });
                document.addEventListener('dragenter', function (e) { return e.preventDefault(); });
                document.addEventListener('dragover', function (e) { return e.preventDefault(); });
                document.addEventListener('dragleave', function (e) { return e.preventDefault(); });
                document.addEventListener('drop', function (e) {
                    e.preventDefault();
                    for (var i = 0; i < e.dataTransfer.files.length; i++) {
                        _this.upload(e.dataTransfer.files[i]);
                    }
                });
                // ペースト
                document.body.addEventListener("paste", function (e) { return _this.handlePaste(e); });
                window.addEventListener("keydown", function (e) {
                    if (document.queryCommandSupported('paste') && e.keyCode == 86 && e.ctrlKey) {
                        document.execCommand('paste');
                    }
                });
                ko.applyBindings(this.viewModel, document.querySelector('#upload-progresses-container'));
            };
            Application.prototype.handlePaste = function (e) {
                var _this = this;
                e.preventDefault();
                var clipboardData = window.clipboardData || e.clipboardData;
                var itemsOrFiles = clipboardData.items || clipboardData.files;
                for (var i = 0; i < itemsOrFiles.length; i++) {
                    var fileOrItem = itemsOrFiles[i];
                    if (fileOrItem.type.match(/^image\//)) {
                        var file = fileOrItem.getAsFile ? fileOrItem.getAsFile() : fileOrItem;
                        setTimeout(function () { return _this.upload(file); }, 1);
                    }
                }
            };
            Application.prototype.upload = function (file) {
                var progressItem = new UploadProgressItem(file.name || 'Untitled');
                this.viewModel.progresses.unshift(progressItem);
                var xhr = new XMLHttpRequest();
                var formData = new FormData();
                xhr.addEventListener('progress', function (e) {
                    //console.log('%d/%d (%d)', e.loaded, e.total, e.loaded / e.total);
                    if (e.lengthComputable) {
                        progressItem.percent((e.loaded / e.total) * 100);
                    }
                    else {
                        progressItem.percent(null);
                    }
                });
                xhr.addEventListener('load', function (e) {
                    progressItem.percent(100);
                    progressItem.imageUrl(JSON.parse(xhr.responseText).Url);
                    progressItem.editUrl(JSON.parse(xhr.responseText).EditUrl);
                });
                formData.append('withXhr', 'true');
                formData.append('uploadedFile', file);
                formData.append('__RequestVerificationToken', this.requestValidationToken);
                xhr.open('POST', this.postUrl, true);
                xhr.send(formData);
            };
            Application.prototype.isBrowserSupport = function () {
                return (window.FormData);
            };
            Application.Current = new Application();
            return Application;
        })();
        Upload.Application = Application;
        var UploadProgressItem = (function () {
            function UploadProgressItem(name) {
                this.percent = ko.observable(null);
                this.name = ko.observable(name);
                this.imageUrl = ko.observable('');
                this.editUrl = ko.observable('');
            }
            return UploadProgressItem;
        })();
        Upload.UploadProgressItem = UploadProgressItem;
    })(Upload = Kanae.Upload || (Kanae.Upload = {}));
})(Kanae || (Kanae = {}));
//# sourceMappingURL=upload.js.map