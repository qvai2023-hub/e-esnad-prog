/// <reference path="../JS/jquery-1.7.2.min.js" />
(function ($) {

    $.fn.QVUploader = function (options) {

        var elem = $(this);
        var Uploader_Id = $(elem).attr('id') + "_QVUploader_";
        var settings = $.extend({

            type: 'png,jpg,jpeg,pdf,doc,docx,xls',
            size: '2',
            path: '/Upload/Company/',
            width: '',
            height: '',
            lang: 'ar',
            displayImage: 'off',
            value: elem.val()
        }, options);



        ///Messages
        extenionMessage = ' إمتداد الملف غير صحيح يجب ان يكون: ' + settings.type + '';
        sizeMessage = settings.size + 'Mb :' + 'خطأ... حجم الملف أكبر من المسموح به وهو '
        errorMessage = 'خطأ... حاول مرة أخرى';
        successMessage = 'لقد تم تحميل الملف بنجاح';
        dimentionMessage = 'خطأ... احداثيات الصورة لابد ان تكون : عرض ' + settings.width + ' * طول ' + settings.height + ' من فضلك ';
        UploadTxt = "تحميل";
        UploadCheck = "تحميل تلقائي:";
        clear = 'مسح';

        //incase if lang = en
        if (settings.lang != "ar") {
            extenionMessage = "This file type is not supported, the supported is: " + settings.type;
            sizeMessage = 'Error... The size of file is more than Permitted which is:' + settings.size + 'Mb';
            errorMessage = 'Error... Please try again';
            successMessage = 'The file uploaded successfully';
            dimentionMessage = 'Error... The dimensions of this image must be ' + settings.width + ' width * ' + settings.height + ' height';
            UploadTxt = "Upload";
            UploadCheck = "Upload on browse:";
            clear = 'Clear';
        }

        //the html of Uploader 
        var Uploader_Html = ' <input  type="hidden" id="' + Uploader_Id + 'IsUploaded" />' +
                           '<input class="fileUpload" id="' + Uploader_Id + 'attach_file" name="" type="file" style="width: 225px;"> <label class="lblUploadCheck" >' + UploadCheck + ' </label>  <input class="chbxUpload" type="checkbox" id="' + Uploader_Id + 'UploadonBrowse" checked="checked" /> <input class="btnNonBrowseUpload" type="button" id="' + Uploader_Id + 'btnUpload" style="display: none" value="' + UploadTxt + '" />' +
                           '<span class="spanUpload" id="' + Uploader_Id + 'upload_label" style="font-size: 10px; color: gray;"></span> <img class="loadedImage"  id="' + Uploader_Id + 'uploadedImage" src="" /> <input id="' + Uploader_Id + 'applicant_file" type="hidden" />' +
                           '<img id="' + Uploader_Id + 'pop_loader" src="/Scripts/QVUploader/images/AjaxLoader.gif" style="display: none;" /> <a id="' + Uploader_Id + 'ClearUpload" class="clearUpload">' + clear + '</a>';

        //hide the used textbox
        elem.css('display', 'none');

        elem.after(Uploader_Html);


        //in case the textbox have value

        if (settings.value != '') {

            $("#" + Uploader_Id + "uploadedImage").attr('src', settings.path + settings.value);
            $("#" + Uploader_Id + "upload_label").text(settings.value).css('color', '#BEC123');
            if (settings.displayImage != 'off') {
                if ($("#" + Uploader_Id + "uploadedImage")[0].Done == false) {
                    $("#" + Uploader_Id + "uploadedImage").attr('src', '/Scripts/QVUploader/images/no_image_thumb.gif');
                }
                $("#" + Uploader_Id + "uploadedImage").show();
            }

        }


        $("#" + Uploader_Id + "UploadonBrowse").change(function (e) {
            e.preventDefault();
            if (!$(this).attr('checked'))
                $("#" + Uploader_Id + "btnUpload").css('display', 'block');
            else
                $("#" + Uploader_Id + "btnUpload").css('display', 'none');

        });

        //when select file to upload
        $("#" + Uploader_Id + "attach_file").live('change', function () {

            var myfile = this;
            if (!$("#" + Uploader_Id + "UploadonBrowse").attr('checked')) {

                $("#" + Uploader_Id + "btnUpload").click(function () {

                    Upload(myfile, settings.type, settings.size, settings.path, settings.width, settings.height, settings.lang, settings.displayImage);
                });
            } else {

                Upload(this, settings.type, settings.size, settings.path, settings.width, settings.height, settings.lang, settings.displayImage);
            }
        });


        //clear value
        $("#" + Uploader_Id + "ClearUpload").bind('click', function (e) {

            e.preventDefault();
            elem.val('');
            $("#" + Uploader_Id + "upload_label").text('');
            $("#" + Uploader_Id + "pop_loader").hide();
            $("#" + Uploader_Id + "applicant_file").val('');
            $("#" + Uploader_Id + "IsUploaded").val('');
            $("#" + Uploader_Id + "uploadedImage").attr('src', '');
            $("#" + Uploader_Id + "uploadedImage").hide();
            $("#" + Uploader_Id + "attach_file").val('');

        });


    };


    function Upload(e, type, size, path, width, height, lang, displayImage) {

        var myTxtbox = $("#" + $(e).attr('id').split('_QVUploader_')[0])
        var Uploader_Id = $(e).attr('id').split('attach_file')[0];


        // in case if press cancel 
        if (e.files.length == 0) {

            $("#" + Uploader_Id + "applicant_file").val('');
        }
        else {

            //get the first file
            var file = e.files[0];

            //check if the file is image and the width and height sent as paramter
            if ((file.type.toLowerCase().indexOf('image') != -1) && (width != '' || height != '')) {

                //create FileReader to get the file url
                var reader = new FileReader();
                reader.onload = (function (theFile) {
                    //create Image and set the src by the reader url
                    var image = new Image();
                    image.src = theFile.target.result;

                    image.onload = function () {
                        // in case if the width and height of the image not equal the sent in paramter, we dont upload and show the message below
                        if (this.width != width || this.height != height) {

                            $("#" + Uploader_Id + "upload_label").html(dimentionMessage).css('color', 'red');

                        } else {

                            Confirm(e, type, size, path, width, height, lang, displayImage, file);
                        }

                    };
                });

                reader.readAsDataURL(file);
            } else {

                Confirm(e, type, size, path, width, height, lang, displayImage, file);
            }

        }
    }


    function Confirm(e, type, size, path, width, height, lang, displayImage, file) {

        var myTxtbox = $("#" + $(e).attr('id').split('_QVUploader_')[0])
        var Uploader_Id = $(e).attr('id').split('attach_file')[0];
        //check if the extention is valid
        if (type.indexOf(file.name.split('.').pop().toLowerCase()) != -1) {

            //check if the size is valid
            //1024 * 1024= 1048576
            if ((size * 1048576) > file.size) {

                $("#" + Uploader_Id + "attach_file").parent().attr({
                    'method': 'POST',
                    'enctype': 'multipart/form-data',
                    'target': '/Scripts/QVUploader/UploadFile.ashx'
                });


                var formdata = false;
                if (window.FormData)
                    formdata = new FormData();

                var file = e.files[0];
                if (formdata) {
                    formdata.append("applicant_file", file);
                    formdata.append("folder", path);
                    $("#" + Uploader_Id + "pop_loader").show();
                    $('input:submit').attr('disabled', 'disabled');
                    $('input:submit').css("opacity", "0.3");
                    $('input:submit').css("cursor", "not-allowed");

                    $("#" + Uploader_Id + "IsUploaded").val("Progress");

                    $.ajax({
                        url: "/Scripts/QVUploader/UploadFile.ashx",
                        type: "POST",
                        data: formdata,
                        processData: false,
                        contentType: false,
                        xhrFields:
                            {
                                onprogress: function (e) {

                                    if (e.lengthComputable) {

                                        //    $("#" + Uploader_Id + "upload_label").html('(Uploading .. ' + (e.loaded / e.total * 100) + '%)').color('orange');
                                    }
                                }
                            },
                        success: function (res) {
                            $("#" + Uploader_Id + "IsUploaded").val("Done");
                            if (file.type.indexOf('image') != -1 && displayImage != "off") {

                                var reader = new FileReader();
                                reader.onload = (function (theFile) {
                                    //create Image and set the src by the reader url
                                    var image = new Image();

                                    image.src = theFile.target.result;

                                    image.onload = function () {
                                        $("#" + Uploader_Id + "uploadedImage").attr('src', image.src);
                                        $("#" + Uploader_Id + "uploadedImage").show();
                                    };
                                });

                                reader.readAsDataURL(file);

                            }
                            var res = eval(res)[0];

                            if (res.status == 'done') {
                                //set my textbox by the filename
                                myTxtbox.val(res.filename)
                                $("#" + Uploader_Id + "upload_label").html(successMessage).css('color', 'green');
                               
                                FormData.file = $("#" + Uploader_Id + "applicant_file").val().trim();
                            }
                            else
                                $("#" + Uploader_Id + "upload_label").html(errorMessage).css('color', 'red');

                            $("#" + Uploader_Id + "pop_loader").hide();
                            $('input:submit').removeAttr('disabled');
                            $('input:submit').css("opacity", "1");
                            $('input:submit').css("cursor", "pointer");
                        },
                        error: function () {
                            $("#" + Uploader_Id + "pop_loader").hide();
                            $("#" + Uploader_Id + "upload_label").html(errorMessage).css('color', 'red');
                        }
                    });
                }
            }
            else {
                $("#" + Uploader_Id + "upload_label").html(sizeMessage).css('color', 'red');

            }
        }
        else {
            $("#" + Uploader_Id + "upload_label").html(extenionMessage).css('color', 'red');
        }
    }
}(jQuery));
