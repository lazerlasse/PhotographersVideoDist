$(document).ready(function () {
    $("#Town").autocomplete({
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
            $("#Postal").val(ui.item.value);
            event.preventDefault();
        }
        //focus: function (event, ui) {
        //    this.value = ui.item.label;
        //    $("#Postal").val(ui.item.value);
        //    event.preventDefault();
        //}
    });
})