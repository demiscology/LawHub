$(document).ready(function () {

    $("#file").on("change", function () {

        var files = $(this)[0].files;
        if (files.length >= 2) {
            $("#label_span").text();
        } else {
            alert("Just one");
        }
    });


});