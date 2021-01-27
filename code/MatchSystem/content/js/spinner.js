// JavaScript source code
var SpinnerFactory = {
    create: function (id) {
        var spinner = {};
        spinner.id = id;
        spinner.index = 0;
        spinner.start = function () {
            $("#" + id).append('<div class="maskpane"><div class="spinner"><div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 26.12px; top: 13.12px; opacity: 0.8125;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 22.12px; top: 22.12px; opacity: 0.8125;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 13.12px; top: 26.12px; opacity: 0.9375;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 4.12px; top: 22.12px; opacity: 0.125;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 0.12px; top: 13.12px; opacity: 0.25;"></div>' +
           '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 4.12px; top: 4.12px; opacity: 0.375;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 13.12px; top: 0.12px; opacity: 0.5;"></div>' +
            '<div class="Spinner-circle" style="height: 5.76px; width: 5.76px; left: 22.12px; top: 4.12px; opacity: 0.625;"></div></div></div>');
             this.optiArray = new Array(0.9375, 0.8125, 0.8125, 0.625, 0.5, 0.375, 0.25, 0.125);
             this.spinnerGoNextStatus(this.index);
        };
        spinner.stop = function(){
            $("#" + this.id + " .maskpane").remove();
            clearTimeout(this.t);
        };
        spinner.spinnerGoNextStatus = function () {
            var rootobj = this;
            var i = this.index;
            $("#" + this.id).find(".Spinner-circle").each(function () {
                if (i > 7) {
                    i = 0;
                }
                $(this).css("opacity", rootobj.optiArray[i++]);
            });
            this.t = setTimeout(function () {
                rootobj.spinnerGoNextStatus();
            }, 100);
            this.index++;
            if (this.index > 7) {
                this.index = 0;
            }
        };
        return spinner;
    }
}

