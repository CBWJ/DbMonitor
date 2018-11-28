//定义一个匿名方法并执行
(function(){
    //扩展新方法
    Array.prototype.contains = function(elem){
        for(var i = 0; i < this.length; ++i){
            if(this[i] == elem) return true;
        }
        return false;
    }
    function SimpleMenu(selector){
        this.selector = selector;
        this.clickCallback = null;
        this.offset = {x:0, y:0};
    }
    SimpleMenu.prototype.init = function(clickCallback){
        var self = this;
        this.clickCallback = clickCallback;
        window.oncontextmenu = function(e){
            //取消默认的浏览器自带右键 很重要！！
            this.console.log("window.oncontextmenu");
        }
        var menu = document.querySelector(self.selector);
        menu.oncontextmenu = function(e){
            e.preventDefault();
        }
        window.onclick = function (e) { 
            var menu = document.querySelector(self.selector);
            menu.style.display = "none";
            console.log("window.onclick");
            }
        this.resetMenuItem();
        return this;
    }
    //禁用菜单项
    SimpleMenu.prototype.disableMenuItem = function (arrOperation){
        if(!arrOperation) arrOperation = [];
        var menu = document.querySelector(this.selector);
        for(var i = 0; i < menu.children.length; ++i){
                var li = menu.children[i];
                var op = li.getAttribute("rel");
                if(arrOperation.contains(op))
                    if(li.className.indexOf("disable") == -1){
                        li.className += " disable";
                        li.onclick = null;
                    }
        }
        return this;
    }
    //重置菜单项
    SimpleMenu.prototype.resetMenuItem = function (){
        var menu = document.querySelector(this.selector);
        var self = this;
        for(var i = 0; i < menu.children.length; ++i){
            var li = menu.children[i];
            li.className = "";
            li.onclick = function (e) {  
                if(self.clickCallback != null && typeof self.clickCallback == "function"){
                    self.clickCallback.call(this);
                }
                self.hide();
                e.cancelBubble = true;
            }
        }
        return this;
    }
    SimpleMenu.prototype.show = function(e){
        var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
        var scrollLeft = document.documentElement.scrollLeft || document.body.scrollLeft;
        var menu = document.querySelector(this.selector);
        menu.style.left = e.clientX + scrollLeft + this.offset.x + "px";
        menu.style.top = e.clientY + scrollTop + this.offset.y + "px";
        menu.style.display = "block";
        return this;
    }
    SimpleMenu.prototype.hide = function(){
        var menu = document.querySelector(this.selector);
        menu.style.display = "none";
        return this;
    }
    window.SimpleMenu = SimpleMenu;
}())