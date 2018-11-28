(function($){
    if(!$) return;
    $.fn.leftmenu = function(options){
        var settings = $.extend({},{clickCallBack:null},options)
        var leftmenu = this;
        //this是一个jQuery对象
        return this.each(function(){
            hideULLoop(this);
            registerClickLoop(this);
        });
        
        function resetSelected(){
            $(this).removeClass("selected");
        }
        function hideLI(){
            $(this).children("UL").hide();
        }
        function showLI(){
            $(this).children("UL").show();
        }

        //递归注册LI标签的点击事件
        function registerClickLoop(ulParent){
            var liNodes = $(ulParent).children("UL").children("LI");
            var ulChilds = liNodes.children("UL");

            liNodes.each(function(){
                $(this).on("click", function(){
                    //在这里不能调用外部函数的变量，是共享的
                    //如果是最终子项触发点击事件
                    resetSelectedRecursive(leftmenu);
                    $(this).toggleClass("selected");
                    if($(this).children("UL").length == 0){
                        if(settings.clickCallBack){
                            settings.clickCallBack.call(this);
                        }
                    }
                    else{
                        $(this).toggleClass("expanded");
                        $(this).children("UL").slideToggle(200);
                        var arrow = $(this).children("a").children().last();
                        arrow.toggleClass("expanded-Arrow");
                    }
                    //阻止事件冒泡
                    return false;
                });
            });
            
            //递归注册子列表UL
            if(ulChilds.length > 0) registerClickLoop(liNodes);
        }

        //递归隐藏UL
        function hideULLoop(ulParent){
            var liNode = $(ulParent).children("UL").children("LI");
            liNode.each(function(){
                hideLI.call(this);
                var ulChild = $(this).children("UL");
                if(ulChild.length > 0) hideULLoop(this);
            });
        }

        function resetSelectedRecursive(ulParent){
            var liNode = $(ulParent).children("UL").children("LI");
            liNode.each(function(){
                resetSelected.call(this);
                var ulChild = $(this).children("UL");
                if(ulChild.length > 0) resetSelectedRecursive(this);
            });
            
        }
    }
}(jQuery));