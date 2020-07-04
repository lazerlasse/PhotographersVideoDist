$(function () {
    for (var r = 0; r < 9; r++) {
        for (var c = 0; c < Model.Count; c++) {
            var titelId = "titel-" + r.toString() + c.toString();
            document.getElementById(titelId).value(Model[c].Titel);
        };
    };
});