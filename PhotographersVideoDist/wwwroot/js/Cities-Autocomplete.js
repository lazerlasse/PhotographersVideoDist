function findCity() {
    $("#locality").autocomplete({
        source: function (requst, response) {
            $.ajax({
                url: "/Cases/GetCities",
                type: "POST",
                dataType: "json",
                data: { prefix: requst.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.label, value: item.value };
                    }));
                }
            });
        },
        selectFirst: true,
        change: function (event, ui) {
            if (ui.item == null) {
                $(this).val((ui.item ? ui.item.label : ""));
            }
        },
        select: function (event, ui) {
            $(this).val(ui.item.label);
            $("#postal_code").val(ui.item.value);
            event.preventDefault();
        }
    });
}

function findPostal() {
    $("#postal_code").autocomplete({
        source: function (requst, response) {
            $.ajax({
                url: "/Cases/GetPostals",
                type: "POST",
                dataType: "json",
                data: { prefix: requst.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.value, value: item.label };
                    }));
                }
            });
        },
        selectFirst: true,
        change: function (event, ui) {
            if (ui.item == null) {
                $(this).val((ui.item ? ui.item.label : ""));
            }
        },
        select: function (event, ui) {
            $(this).val(ui.item.label);
            $("#locality").val(ui.item.value);
            event.preventDefault();
        }
    });
}