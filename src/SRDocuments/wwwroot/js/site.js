var i = 0;
var bP = false;

$(function () {

    // We can attach the `fileselect` event to all file inputs on the page
    $(document).on('change', ':file', function () {
        var input = $(this),
            numFiles = input.get(0).files ? input.get(0).files.length : 1,
            label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
        input.trigger('fileselect', [numFiles, label]);
    });

    // We can watch for our custom `fileselect` event like this
    $(document).ready(function () {
        $(':file').on('fileselect', function (event, numFiles, label) {

            var input = $(this).parents('.input-group').find(':text'),
                log = numFiles > 1 ? numFiles + ' files selected' : label;

            if (input.length) {
                input.val(log);
            } else {
                if (log) alert(log);
            }

        });
    });

});

$(document).keydown(function (event) {
    if (i == 0 && event.which == 38) {
        i++;
    }
    else if (i == 1 && event.which == 38) {
        i++;
    }
    else if (i == 2 && event.which == 40) {
        i++;
    }
    else if (i == 3 && event.which == 40) {
        i++;
    }
    else if (i == 4 && event.which == 37) {
        i++;
    }
    else if (i == 5 && event.which == 39) {
        i++;
    }
    else if (i == 6 && event.which == 37) {
        i++;
    }
    else if (i == 7 && event.which == 39) {
        i++;
    }
    else if (i == 8 && event.which == 66) {
        i++;
    }
    else if (i == 9 && event.which == 65) {
        if (bP) {
            bP = false;
            document.body.style.backgroundImage = "";
        }
        else {
            bP = true;
            document.body.style.backgroundImage = "url('../img/bkgIe.jpg')";
            document.body.style.backgroundSize = "100%";
        }
        i = 0;
    }
    else {
        i = 0;
    }
});

$(document).ready(checkboxChanged());

function checkboxChanged() {
    if ($("#CheckRequiredDate").is(":checked")) {
        $("#divRequiredDate").show();
    }
    else {
        $("#divRequiredDate").hide();
    }
}

$("#vamoLa").on('click', function () {
    $("#rodou").text($(this).data('id'));
    $("#basicExample").modal('show');
});

$("#requestBlockModalOpen").on('click', function () {
    $("#blockModalCloseButton").text("Cancel");
    $("#sendBlockRequest").show();
    $("#modalBodyResponse").html("<center>Would you really like to block your account?</center>");
    $("#requestBlockModal").modal('show');
});

$("#sendBlockRequest").on('click', function () {
    document.addEventListener("click", handler, true);
    $(this).hide();
    $("#blockModalCloseButton").hide();
    $("#modalBodyResponse").html("<center><i class='fa fa-spin fa-spinner fa-3x'></i></center>");
    $.ajax({
        type: "GET",
        url: "/Manage/RequestBlockAccount",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        complete: function (data) {
            $("#blockModalCloseButton").text("Close");
            $("#blockModalCloseButton").show();
            $("#modalBodyResponse").html("<center>" + data.responseText + "</center>");
            document.removeEventListener("click", handler, true);
        },

    });
});

function deleteModalOpener(event) {
    $("#deleteValueInput").val($(event).data('id'));

    $("#deleteInfo").html(
            '<dt class="col-sm-3 text-md-right">Id:</dt>' +
            '<dd class="col-sm-9">' + $(event).data('id') + '</dd>' +
            '<dt class="col-sm-3 text-md-right">Name:</dt>' +
            '<dd class="col-sm-9 text-truncate">' + $(event).data('docname') + '</dd>' +
            '<dt class="col-sm-3 text-md-right">Sent to:</dt>' +
            '<dd class="col-sm-9">' + $(event).data('sentto') + '</dd>');

    $("#deleteDocumentModal").modal('show');
}

function acceptModalOpener(event) {
    $("#acceptValueInput").val($(event).data('id'));
    $("#acceptValueSpan").text($(event).data('id'));
    $("#acceptDocumentModal").modal('show');
}

function denyModalOpener(event) {
    $("#denyValueInput").val($(event).data('id'));
    $("#denyValueSpan").text($(event).data('id'));
    $("#denyDocumentModal").modal('show');
}

function handler(e) {
    e.stopPropagation();
    e.preventDefault();
}