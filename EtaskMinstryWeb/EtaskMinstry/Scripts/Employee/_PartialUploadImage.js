    $(function () {
        ////upload employee image
        // To View Browse Window .
        $(".upload").live('click', function (e) {
            debugger;
            var elem = $(e.target);
            e.preventDefault();
            var parentdiv = $(this).parent().closest('div[id]').attr("id");
            $(this).hide();
            $("#" + parentdiv + ' .btnCancelFile').show();
            $("#" + parentdiv + " .fileImage").click();
            elem.unbind('click');
        });
        $('.btnCancelFile').click(function () {
            var parentdiv = $(this).parent().closest('div[id]').attr("id");
            if (parentdiv == "empImgname") {
                $("#" + parentdiv + " #EmpImageName").attr("src", "/Content/Main/images/default-avatar.jpg");
                $("#ImageName").val("");
            } else {
                $("#" + parentdiv + " #EmpNationaIDImage").attr("src", "/Content/Main/images/default-avatar.jpg");
                $("#NationaIDImage").val("");
            }
            $("#" + parentdiv + ' .btnCancelFile').hide();
            $("#" + parentdiv + ' .fileImage').val('');
            $("#" + parentdiv + " .upload").show();
        });
        $('.fileImage').change(function (evt) {
            //////////////////////////// for Update Img Src When FileUpload Changed

            var parentdiv = $(this).parent().closest('div[id]').attr("id");
            var files = evt.target.files;
            //var Extentions = "@System.Configuration.ConfigurationManager.AppSettings['FileExtentions']";
            //var size = "@System.Configuration.ConfigurationManager.AppSettings['maxfileSize']";
            var fileType = "";
            var picReader = new FileReader();
            if (files.length > 0) {
                debugger;
                $("#" + parentdiv + ' .load').show()
                picReader.readAsBinaryString(files[0]);
            } else {
                if (parentdiv == "empImgname") {
                    $("#" + parentdiv + ' #EmpImageName').val("");
                    $("#ImageName").val("");
                } else {
                    $("#" + parentdiv + ' #EmpNationaIDImage').val("");
                    $("#NationaIDImage").val("");
                }

                $("#" + parentdiv + ' .UploadErrorMsg').text('');
            }
            picReader.onload = function (e) {
                fileType = e.target.result.trim().substring(0, 10);

                if (files.length > 0) {
                    var ext = files[0].name.split('.').pop().toLowerCase();
                    debugger;
                    var bReturn = CheckIsValid(Extentions, ext, fileType);
                    if (bReturn == false) {
                        $("#" + parentdiv + ' .UploadErrorMsg').text('غير مسموح بنوع الملف').css('color', 'red');

                        return false;
                    }
                    else if (files[0].size > size) {
                        $("#" + parentdiv + ' .UploadErrorMsg').text('يجب ان لا يتجاوز حجم الملف 4 ميجا').css('color', 'red');

                        return false;
                    }
                    else {
                        debugger;
                        var formdata = new FormData();
                        formdata.append(files[0].name, files[0]);
                        $.ajax({
                            async: true,
                            cache: false,
                            contentType: false,
                            processData: false,
                            type: "POST",
                            url: '/Admin/Employee/AddImage',
                            data: formdata,
                            success: function (response) {
                                if (parentdiv == "empImgname") {
                                    $("#EmpImageName").attr("src", response);
                                    $('#ImageName').val(response.replace('/Upload/Employee/', ''));
                                } else {
                                    $("#EmpNationaIDImage").attr("src", response);
                                    $('#NationaIDImage').val(response.replace('/Upload/Employee/', ''));
                                }
                                $("#" + parentdiv + ' .UploadErrorMsg').text('');
                                $("#" + parentdiv + ' .UploadErrorMsg').text('تم رفع الملف بنجاح').css('color', 'green');
                                $("#" + parentdiv + ' .load').hide()
                            },
                            error: function (e) {
                            }
                        });
                    }
                }
            };


            $.validator.addMethod("maxfilesize", function (value, element) {
                return element.files[0].size < 1024 * 1024 * 2;
            }, 'حجم هذا الملف لايمكن ان يتجاوز 2 ميجابايت');

            $.validator.addMethod("limitfiletype", function (value, element) {

                var ext = value.split('.').pop().toLowerCase();
                return ext == 'jpg' || ext == 'jpeg' || ext == 'png';
            }, 'امتداد هذا الملف غير مسموح به');
        });
    });
function CheckIsValid(Extentions, ext, fileType) {
    var valid = false;

    if (Extentions.indexOf(ext) >= 0) {
        //for pdf, PDF
        if (fileType.indexOf("%PDF") >= 0) {
            valid = true;
        }
            //DOCX, PPTX, XLSX
        else if (fileType.indexOf("PK") >= 0) {
            valid = true;
        }
            //DOC, DOT, PPS, PPT, XLA, XLS, WIZ
        else if (fileType.indexOf("ÐÏ.à¡±.á") >= 0 || fileType.indexOf("ÐÏà¡±á") >= 0) {
            valid = true;
        }
            //JFIF, JPE, JPEG, JPG
        else if (fileType.indexOf("ÿØÿà") >= 0) {
            valid = true;
        }
            //PNG
        else if (fileType.indexOf("PNG") >= 0) {
            valid = true;
        }

    }
    else { valid = false; }
    return valid;
}
