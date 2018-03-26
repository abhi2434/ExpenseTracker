var controller = "Edit/";


//function doEditApp(appId, obj) {
//    var data = { AppId: appId }
//    if (lockState == false) {
//        msg = 'Do you want to unlock the Organization for further changes?'
//    }
//    var uri = baseUrl + controller + "ResetLock";
//    if (confirm(msg)) {
//        var dataObj = JSON.stringify(data);
//        $.ajax({
//            dataType: "json",
//            contentType: "application/json",
//            type: "POST",
//            url: uri,
//            data: dataObj,
//            error: function (e) {
//                location.hash = "close";
//                location.reload();
//            },
//            success: function (data) {
//                location.hash = "close";
//                location.reload();
//            }
//        });
//    }
//}