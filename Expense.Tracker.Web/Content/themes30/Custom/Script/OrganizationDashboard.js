var controller = "Subcription/";

$(function () {
    $("[id^='datepicker']").datepicker({ minDate: "-12M", maxDate:"+12Y" });
});




function changeExpiry(subscriptionId, obj) {
    debugger;
    var expiryDate = $("#datepicker_" + subscriptionId).val();
    var active = $("#activateCheckbox_" + subscriptionId).is(":checked");

    var uri = baseUrl + controller + "ChangeExpiry?id=" + subscriptionId + "&expiryDate=" + expiryDate + "&active=" + active;
    $.ajax({
        dataType: "json",
        data:"",
        url: uri,
        error: function (e) {
            location.hash = '';
            //location.reload();
            $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
        },
        success: function (data) {
            location.hash = '';
            //location.reload();
            $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
        }
    });
}

function doResetAgents(orgId, obj) {
    var msg = 'Do you want to deactivate all unlicenced agents?';
    var uri = baseUrl + controller + "ResetAgents?id=" + orgId;
    if (confirm(msg)) {
        $.ajax({
            dataType: "json",
            url: uri,
            error: function (e) {
                location.reload();
            },
            success: function (data) {
                location.reload();
            }
        });
    }
}

function doTestOrg(orgId, marker, obj) {
    var msg = 'Do you want to mark this organization as test?';
    var uri = baseUrl + controller + "ResetTestOrg?id=" + orgId + "&doMarkTest=" + marker;
    if (confirm(msg)) {
        $.ajax({
            dataType: "json",
            url: uri,
            error: function (e) {
                location.reload();
            },
            success: function (data) {
                location.reload();
            }
        });
    }
}

function doLock(orgId, obj, lockState) {
    var msg = 'Do you want to lock the Organization for further changes?'
    var data = { OrgId: orgId, IsLocked: lockState }
    message = $("#ta_" + orgId).val();
    msgTitle = $("#tb_" + orgId).val();
    if (lockState == false) {
        msg = 'Do you want to unlock the Organization for further changes?'
    }
    data["MessageTitle"] = msgTitle;
    data["MessageText"] = message;
    var uri = baseUrl + controller + "ResetLock";
    if (confirm(msg)) {
        var dataObj = JSON.stringify(data);
        $.ajax({
            dataType: "json",
            contentType: "application/json",
            type: "POST",
            url: uri,
            data: dataObj,
            error: function (e) {
                location.hash = "close";
                location.reload(true);
            },
            success: function (data) {
                location.hash = "close";
                location.reload(true);
            }
        });
    }
}

function doActivateDate(subscriptionId, obj, days) {
    var msg = 'Do you want to activate the subscription?'
    var uri = baseUrl + controller + "MakeActive?id=" + subscriptionId
    if (days) {
        msg = 'Do you want to activate the subscription for ' + days + ' days?';
        uri += '&days=' + days
    }
    if (confirm(msg)) {
        $.ajax({
            dataType: "json",
            url: uri,
            error: function (e) {
                location.reload();
            },
            success: function (data) {
                location.reload();
            }
        });
    }
}

function doActivate(subscriptionId, obj, days) {
    var msg = 'Do you want to activate the subscription?'
    var uri = baseUrl + controller + "MakeActive?id=" + subscriptionId
    if (days) {
        msg = 'Do you want to activate the subscription for ' + days + ' days?';
        uri += '&days=' + days
    }
    if (confirm(msg)) {
        $.ajax({
            dataType: "json",
            url: uri,
            error: function (e) {
                //location.reload();
                $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
            },
            success: function (data) {
                //location.reload();
                $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
            }
        });
    }
}

function doDeActivate(subscriptionId, obj) {
    if (confirm('Do you want to deactivate the subscription?')) {
        $.ajax({
            dataType: "json",
            url: baseUrl + controller + "MakeDeActive?id=" + subscriptionId,
            error: function (e) {
                //location.reload();
                $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
            },
            success: function (data) {
                //location.reload();
                $("#SubscriptionDetailsPartialView").load(location.href + " #SubscriptionDetailsPartialView");
            }
        });
    }
}


function reloadSubscriptionDetails() {

}