var DialogFactory = {
    create:function(container, contentHTML){
        var dialog = {};
        dialog.container = container;
        dialog.contentHTML = contentHTML;
        var html = "<div class=\"dialogbox\">" +
                     "<span class=\"closebutton\"></span>"+
                     "<div class=\"dialogboxcontentcontainer\"></div>"+
                   "</div>";
        if (dialog.container == null) {
            $("body").append(html);
        }
        else {
            $("#" + container).append(html);
        }

        $(".dialogbox .closebutton").click(function () {
            dialog.destroy();
        });
        $(".dialogboxcontentcontainer").append(dialog.contentHTML);
        dialog.show = function () {
            $(".dialogbox").show();
        };
        dialog.hide = function () {
            $(".dialogbox").hide();
        };
        dialog.destroy = function () {
            $(".dialogbox").remove();
        };
        return dialog;
    }

}
