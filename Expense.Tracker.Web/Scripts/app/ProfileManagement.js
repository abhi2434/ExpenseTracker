$(document).ready(function () {

    $('#fileupload').fileupload({
        url: '/Profile/FileUpload',
        dataType: 'json',
        formData: { id: $('#userId').val() },
        add: function (e, data) {
            $('#progress').show();
            //data.id = $('#userId').val();
            data.submit();
        },
        done: function (e, data) {
            var result = data.result;
            if (result != '') {
                $('#ProfilePic').val(result);
                $('#ImagePath,#PicPath,#gUserPic').attr('src', result);
            }
            $('#progress').hide();
        },
        progressall: function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('#progress .progress-bar').css(
                'width',
                progress + '%'
            );
        }
    });

});
function removeImage() {
    $.ajax({
        url: '/Profile/RemoveFile',
        datatype: 'json',
        data: { id: $('#userId').val() },
        type: 'POST',
        success: function (data) {
            $('#ImagePath,#PicPath').attr('src', 'http://www.placehold.it/200x150/EFEFEF/AAAAAA&amp;text=no+image');
        }
    });
}
function updateUserDetails() {
    debugger;
    var fullName = $('#appFullName').val();
    if (fullName == '') {
        alert('Full name is mandtory !');
        return;
    }
    var contactNo = $('#appContact').val();
    var param = { "userName": fullName, "contactNumber": contactNo };
    $.ajax({
        url: baseUrl + 'Profile/EditDetails',
        data: param,
        success: function (data) {
            if (data) {
                var response = data["Response"];
                toastr.success(response, 'Success');
            }
        },
        error: function (e) {
            toastr.error(e.responseText, 'Sorry!');
        }
    });
}
function updateUserPassword() {
    debugger;
    var oldPassword = $('#oldpassword').val();
    if (oldPassword.length == 0) {
        alert('Please enter old password !');
        return;
    }
    var newPassword = $('#newpassword').val();
    if (newPassword.length == 0) {
        alert('Please enter new password !');
        return;
    }
    var reenterPassword = $('#reenterpassword').val();
    if (reenterPassword.length == 0) {
        alert('Please enter reenter password !');
        return;
    }
    if (newPassword != reenterPassword) {
        alert('Password mismatch, the password entered does not match !');
        return;
    }
    var param = { "oldPassword": oldPassword, "newPassword": newPassword };
    $.ajax({
        url: baseUrl + 'Profile/EditPassword',
        data: param,
        success: function (data) {
            if (data) {
                var response = data["Response"];
                toastr.success(response, 'Success');
            }
        },
        error: function (e) {
            toastr.error(e.responseText, 'Sorry!');
        }
    });
}
function NotImplemented() {
    toastr.info('The feature is not yet implemented, please try again later', 'Not implemented');
}