/// <reference path="D:\01_work\intoinventory\code\into.inventory.homepc\Into.Inventory.UI\AdminLTE-2.3.0/plugins/jQuery/jQuery-2.1.4.min.js" />

;
(function ($)
{
    $.fn.zPagination = function (options)
    {
        var defaults = {
            // 当前页码
            PageIndex: 1,
            // 总页码数
            PageCount: 1,
            // 显示按钮个数
            ButtonNum: 5,
            // 点击页码回调函数
            CallBack: function () { }
        };
        var opts = $.extend({}, defaults, options);

        // 绘制分页
        var drawPage = function (thisObj, currentIndex, totalPages)
        {
            var $pageContainer = $(thisObj),
                btnNum = opts.ButtonNum,
                cIndex = currentIndex,
                tPages = totalPages;

            var pageHtmlStart = "<div class='z-paginationjs' >" + "<div class='z-paginationjs-pages'>",
                pageHtmlEnd = " <div class='z-paginationjs-go-input'>跳转<input type='text' class='z-paginationjs-pagenum'>页</div><div class='z-paginationjs-go-button'><input type='button' value='Go'></div>",
                htmlBody = "";

            htmlBody = pageHtmlStart + "<ul>";
            // 上一页按钮
            htmlBody += "<li class='z-paginationjs-pre' data-num='" + (cIndex - 1) + "' title='上一页'><a href='javascript:;'>«</a></li>";

            // 绘制按钮
            if (tPages < btnNum + parseInt(btnNum / 2))
            {
                for (var i = 1, total = tPages; i <= total; i++)
                {
                    htmlBody += "<li class='z-paginationjs-page' data-num='" + i + "'><a href='javascript:;'>" + i + "</a></li>";
                }
            }
            else if (cIndex - parseInt(btnNum / 2) <= 1)
            {

                for (var j = 1; j <= btnNum; j++)
                {
                    htmlBody += "<li class='z-paginationjs-page' data-num='" + j + "'><a href='javascript:;'>" + j + "</a></li>";
                }
                // 省略
                htmlBody += "<li class='z-paginationjs-ellipsis disabled'><a href='javascript:;'>...</a></li>";
                // 最后一页
                htmlBody += " <li class='z-paginationjs-page' data-num='" + tPages + "'><a href='javascript:;'>" + tPages + "</a></li>";

            }
            else if (cIndex + parseInt(btnNum / 2) >= (tPages - 1))
            {
                // 第一页
                htmlBody += "<li class='z-paginationjs-page' data-num='1' ><a href='javascript:;'>1</a></li>";
                // 省略
                htmlBody += "<li class='z-paginationjs-ellipsis disabled'><a href='javascript:;'>...</a></li>";
                for (var m = 1; m <= btnNum; m++)
                {
                    htmlBody = htmlBody + "<li class='z-paginationjs-page' data-num='" + (tPages - (btnNum - m)) + "'><a href='javascript:;'>" + (tPages - (btnNum - m)) + "</a></li>";
                }
            }
            else
            {

                // 第一页
                htmlBody += "<li class='z-paginationjs-page' data-num='1' ><a href='javascript:;'>1</a></li>";
                // 省略
                htmlBody += "<li class='z-paginationjs-ellipsis disabled'><a href='javascript:;'>...</a></li>";

                for (var n = 0; n < btnNum; n++)
                {

                    htmlBody = htmlBody + "<li class='z-paginationjs-page' data-num='" + (cIndex - parseInt(btnNum / 2) + n) + "'><a href='javascript:;'>" + (cIndex - parseInt(btnNum / 2) + n) + "</a></li>";
                }

                // 省略
                htmlBody += "<li class='z-paginationjs-ellipsis disabled'><a href='javascript:;'>...</a></li>";
                // 最后一页
                htmlBody += " <li class='z-paginationjs-page' data-num='" + tPages + "'><a href='javascript:;'>" + tPages + "</a></li>";

            }

            // 下一页
            htmlBody += " <li class='z-paginationjs-next' data-num='" + (cIndex + 1) + "' title='下一页'><a href='javascript:;'>»</a></li>";
            htmlBody += "</ul></div>";

            htmlBody += pageHtmlEnd;

            // 绘制
            $pageContainer.html(htmlBody);

            // 设置按钮状态

            var preNum = $pageContainer.find('.z-paginationjs-pre').data('num'),
                nextNum = $pageContainer.find('.z-paginationjs-next').data('num'),
                liList = $pageContainer.find('.z-paginationjs-pages ul li.z-paginationjs-page');
            if (liList.length)
            {
                $.each(liList, function (i, e)
                {
                    var $thisElement = $(e);
                    if ($thisElement.data('num') == currentIndex)
                    {
                        $thisElement.addClass('active');
                    }

                });
            }
            if (preNum < 1)
            {
                $pageContainer.find('.z-paginationjs-pre').addClass('disabled');
            }
            if (nextNum > tPages)
            {
                $pageContainer.find('.z-paginationjs-next').addClass('disabled');
            }
        };

        // 分页按钮事件
        var addEvent = function (thisObj)
        {

            var $obj = $(thisObj),
                $pages = $obj.find('div.z-paginationjs div.z-paginationjs-pages'),
                $goBtn = $obj.find('div.z-paginationjs div.z-paginationjs-go-button'),
                $inputElm = $obj.find('div.z-paginationjs div.z-paginationjs-go-input input[type="text"]');
            //console.log($obj);
            //console.log($pages);
            //console.log($goBtn);
            // 页码按钮
            $pages.on('click', 'li', function ()
            {
                var $thisLi = $(this),
                    liNum = 0;
                if ($thisLi.hasClass('active') || $thisLi.hasClass('disabled'))
                    return;
                liNum = $thisLi.data('num');
                if (liNum && liNum <= opts.PageCount && liNum > 0)
                {

                    //回调函数
                    opts.CallBack(liNum);
                }
            });

            // 跳转按钮
            $goBtn.on('click', 'input[type="button"]', function ()
            {
                var $this = $(this);

                console.log($inputElm.val());

                var _goNum = parseInt($inputElm.val());

                if (_goNum > 0 && (_goNum <= opts.PageCount) && (opts.PageIndex != _goNum))
                {

                    opts.CallBack(_goNum);

                } else
                {
                    $inputElm.addClass('error');
                }

            });
            $inputElm.on('focus', function ()
            {
                var $this = $(this);
                $this.removeClass('error');
            });
        };

        function setStyle()
        {

            var styleElementt = $('#zpaginationjs-style');
            if (styleElementt.length)
                return;

            var cssText = '.z-paginationjs{line-height:1.6;font-family:"Marmelad","Lucida Grande","Arial","Hiragino Sans GB",Georgia,sans-serif;font-size:14px;box-sizing:initial}.z-paginationjs .z-paginationjs-pages{float:left}.z-paginationjs .z-paginationjs-pages ul{float:left;margin:0;padding:0}.z-paginationjs .z-paginationjs-pages li{float:left;border:1px solid #aaa;border-right:0;list-style:none}.z-paginationjs .z-paginationjs-pages li a{border:0}.z-paginationjs .z-paginationjs-pages li:first-child{border-radius:3px 0 0 3px}.z-paginationjs .z-paginationjs-pages li:last-child{border-right:1px solid #aaa;border-radius:0 3px 3px 0}.z-paginationjs .z-paginationjs-pages li>a{min-width:30px;height:28px;line-height:28px;display:block;background:#fff;font-size:14px;color:#333;text-decoration:none;text-align:center}.z-paginationjs .z-paginationjs-pages li>a:hover{background:#eee}.z-paginationjs .z-paginationjs-pages li.active>a{line-height:30px;background:#aaa;color:#fff}.z-paginationjs .z-paginationjs-pages li.disabled>a{opacity:.3;cursor:no-drop}.z-paginationjs .z-paginationjs-ellipsis li{float:left;border:1px solid #aaa;border-right:0;list-style:none}.z-paginationjs .z-paginationjs-go-input{float:left;margin-left:10px;font-size:14px}.z-paginationjs .z-paginationjs-go-input>input[type="text"]{width:30px;height:28px;background:#fff;border-radius:3px;border:1px solid #aaa;padding:0;font-size:14px;text-align:center;vertical-align:baseline;outline:0;box-shadow:none;box-sizing:initial}.z-paginationjs .z-paginationjs-go-button{float:left;margin-left:10px;font-size:14px}.z-paginationjs .z-paginationjs-go-button>input[type="button"]{min-width:40px;height:30px;line-height:28px;background:#fff;border-radius:3px;border:1px solid #aaa;text-align:center;padding:0 8px;font-size:14px;vertical-align:baseline;outline:0;box-shadow:none;color:#333;cursor:pointer}.z-paginationjs .z-paginationjs-go-button>input[type="button"]:hover{background:#eee}.z-paginationjs .error{border: 1px solid red !important}';

            $('head').append('<style type="text\/css" id="zpaginationjs-style">' + cssText + '<\/style>');
        }

        function checkOptions()
        {
            if (typeof (opts.CallBack) !== 'function')
            {
                console.log("参数CallBack必须为函数");
                return false;
            }
            if (opts.PageIndex > opts.PageCount)
            {
                console.log("当前页必须小于总页码");
                return false;
            }
            return true;
        }

        this.each(function ()
        {
            var thisObj = this;
            if (checkOptions())
            {
                // 设置样式
                setStyle();
                // 绘制分页
                drawPage(thisObj, opts.PageIndex, opts.PageCount);
                // 添加绑定事件
                addEvent(thisObj);
            }
            else
            {
                console.log("参数不正确");

                var $this = $(thisObj);
                $this.empty();
                $this.append('<div class="z-paginationjs-go-input">没有数据！</div>')
            }
        });
    };

})(jQuery);
