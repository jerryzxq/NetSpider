function generatePositionJson(ele)
{
    var result = "{'offsetLeft':" + getElementLeft(ele) + ", 'offsetTop':" + getElementTop(ele) + "}";
    return result;
}

function getElementLeft(element)
{
    var actualLeft = element.offsetLeft;
    var current = element.offsetParent;

    while (current !== null)
    {
        actualLeft += current.offsetLeft;
        current = current.offsetParent;
    }

    return actualLeft;
}

function getElementTop(element)
{
    var actualTop = element.offsetTop;
    var current = element.offsetParent;

    while (current !== null)
    {
        actualTop += current.offsetTop;
        current = current.offsetParent;
    }

    return actualTop;
}

document.getElementsByClassName = function (cl)
{
    var retnode = [];
    var elem = this.getElementsByTagName('*');
    for (var i = 0; i < elem.length; i++)
    {
        if ((' ' + elem[i].className + ' ').indexOf(' ' + cl + ' ') > -1) retnode.push(elem[i]);
    }
    return retnode;
}; 