
function ajaxRequest(type, url, data, dataType, percentage) {
    var options = {

        dataType: dataType || "json",
        contentType: "application/json",
        cache: false,
        type: type,
        data: data ? ko.toJSON(data) : null
       
    };
    var accessToken = sessionStorage["accessToken"] || localStorage["accessToken"];
    if (accessToken) {
        options.headers = {
            "Authorization": "Bearer " + accessToken
        }
    }
    
    return $.ajax(url, options);
}

var calls = {
    ajaxCalls: 1,
    thiscallback: 0,
    reportEnd: function () {
        calls.thiscallback++;
        var percentage = (calls.thiscallback / calls.ajaxCalls) * 100;
        $("#progress .bar").width(percentage + "%");
        if (calls.ajaxCalls == calls.thiscallback) {
            $("#progress").hide();
        }
        else {
            $("#progress").css('display', 'block');
            $("#progress").show();
        }

    }
};
function ajaxSetup(totalCalls)
{
    calls.ajaxCalls = totalCalls;
    calls.thiscallback = 0;
    return calls;
}

var startProgressIntreval = null;
$(document).bind("ajaxSend", function () {
    $("#progress").show();
   startProgressIntreval= setInterval(function () {
       var currentWidth = Math.round($(".bar").width() / $("#progress").width() * 100)
       if (currentWidth < 80) {
           $("#progress .bar").width(Math.round(currentWidth + 10) + '%');
       } else {
           clearInterval(startProgressIntreval);
       }        
    }, 3000)
}).bind("ajaxComplete", function () {
    //$("#progress").hide(1000, function () {
    //   // console.log('callback');
    //    clearInterval(startProgressIntreval);
    //});
    $("#progress").hide();
   
       
});


//var baseUrl = "http://localhost:1326/";
//var baseUrl = "http://115.119.201.197/Application/";
//var baseUrl = "http://192.168.1.98:8081/Application/";
