/**
 * Lock.js - модуль для блокировки страницы.
 *
 * Модуль предоставляет возможности для блокирования странички на
 * время выполнения операции. Полезно при работе с ajax, когда
 * страница не перегружается во время операции. Может быть также
 * полезна и при любых обращениях с сервером, потому что при нажатии
 * на кнопку или ссылку страничка не исчезает и пользователь может
 * успеть нажать еще какие-нибудь кнопки.
 *
 * |----------------------|
 * | Пример использования:|
 * |----------------------|
   <script type='text/javascript' src='js/nkmk/Lock.js'></script>

    <script>
      function test(){
        if(nkmk.Lock.isFree()){
          nkmk.Lock.setBusy();
          setTimeout(function(){nkmk.Lock.setFree()}, 10000);
          for(var i = 0; i < 10; i++){
            setTimeout("nkmk.Lock.printBusyMessage('" + (10 - i) + " seconds left');", i * 1000);
          }
        }
      }
    </script>
    <input type="button" onclick="test()" value="setBusyForTenSeconds"/>
 *
 * На 10 секунд заблокирует страничку и каждую секунду будет выдавать
 * сообщение о количестве оставшихся секунд.
 */


/* Создание "пространства имен":
 * Определяется единственная глобальная переменная */
var nkmk;

if (!nkmk) {//если такой объект не существует
	nkmk = {};//то создаём новый пустой объект
} else if (typeof nkmk != "object") {//если уже определена глобальная переменная nkmk и она не является объктом
	throw new Error("nkmk already exists and is not an object");//выдать ошибку
}

if (!nkmk.Lock) {//если такой объект не существует
	nkmk.Lock = {};//то создаём новый пустой объект
}else if (typeof nkmk.Lock != "object"){//если данное поле определено и не является объектом
	throw new Error("nkmk.Lock already exists and is not an object");//выдать ошибку
}

/**
 * Флаг, определяющий выполняются ли какие-нибудь операции (если
 * false, то выполняются).  Функции, для которых необходимо
 * блокирование остальных функций, должны проверять значение данной
 * переменной, используя метод nkmk.Lock.isFree() и должны
 * выполняться, только если переменная равна true
 */

nkmk.Lock.__isFree = true;

/**
 * Установить блокировку. Данная функция должна вызываться во всёх
 * функциях, которым важна блокировка сразу после проверки
 * if(nkmk.Lock.isFree())
 * @param style (optional) - стиль блокировки
 */
nkmk.Lock.setBusy = function(/*optional*/style){
	this.__isFree = false;

	if(style != null){
		if(style == "simple"){
			/* Отмена всех кликов и нажатий клавиш на странице на время выполнения операции*/
			document.onmousedown = function(){return false;};//перехват кликов
			document.onkeydown = function(){return false;};//перехват нажатий
//			nkmk.Lock.__style = "simple";
		}
	}else{



		document.body.style.cursor = "progress"; //меняем курсор на часики

		/* Отмена всех кликов и нажатий клавиш на странице на время выполнения операции*/
		document.onmousedown = function(){return false;};//перехват кликов
		document.onkeydown = function(){return false;};//перехват нажатий

		//iframe нужно создавать, чтобы перекрыть элементы <select>
		var iframe = document.createElement("iframe");
		iframe.style.position = "absolute";
		iframe.style.frameborder = "0";
		iframe.style.scrolling = "0";
		iframe.style.top = "0";
		iframe.style.left = "0";
		iframe.style.width = document.body.scrollWidth;
		iframe.style.height = window.screen.availHeight;
		iframe.style.zIndex = "10050";
		iframe.style.filter = 'alpha(opacity=50)';

		//div внешний див, содержищий картинку и область для сообщений
		var div = document.createElement("div");
		div.style.position = "absolute";
		div.style.left = "0";
		div.style.top = "0";
		div.style.width = document.body.scrollWidth;
		div.style.height = document.body.scrollHeight;
		div.style.zIndex = "10051";
		div.align = "center";
		div.innerHTML = "<img style='position: absolute; top: " + document.body.clientHeight / 2 + "' src='Content/Images/process_animation.gif'/>";

		//область для сообщений
		var innerDiv = document.createElement("div");
		innerDiv.style.position = "absolute";
		innerDiv.style.left = "0";
		innerDiv.style.top = document.body.scrollHeight / 2 + 32;
		innerDiv.style.width = document.body.clientWidth;
		innerDiv.style.zIndex = "10051";
		innerDiv.align = "center";
		innerDiv.style.fontFamily = "arial";
		innerDiv.style.fontSize = "12px";
		innerDiv.style.color = "navy";
		innerDiv.style.fontWeight = "bold";

		nkmk.Lock.busyIFrame = iframe;//свойству busyIFrame присваивается ссылка на iframe, чтобы не заморачиваться с id
	  //а то пришлось бы придумывать id, а его могут занять в другой элемент
		nkmk.Lock.busyDiv = div;//тоже самое
		div.messageArea = innerDiv;

		nkmk.Base.insertFirstChild(document.body, iframe);
		nkmk.Base.insertFirstChild(document.body, div);
		nkmk.Base.insertFirstChild(div, innerDiv);
	}
}

/**
 * Снять блокировку. Данная функция должна вызываться во всёх
 * функциях, которым важна блокировка сразу после окончания их
 * выполнения.  Если функция делает submit формы или меняет свойство
 * document.location, то вроде бы можно не вызывать данную
 * функцию. Проверено на изменении свойства document.location.
 */
nkmk.Lock.setFree = function (){
	var iframe = nkmk.Lock.busyIFrame;
	var div = nkmk.Lock.busyDiv;
	if(div != null){
		div.parentNode.removeChild(div);//удаляем div из документа
		nkmk.Lock.busyDiv = null;
	}
	if(iframe != null){
		iframe.parentNode.removeChild(iframe);//удаляем frame из документа
		nkmk.Lock.busyIFrame = null;
	}
	this.__isFree = true;
	document.onmousedown = function(){};//разрешаем клики
	document.onkeydown = function(){};//разрешаем нажатия клавиш
  document.body.style.cursor = "default";//делаем обычным курсор
}

/**
 * Определить не выставлена ли блокировка, если блокировки нет
 * возвращает true
 */
nkmk.Lock.isFree = function (){
	return this.__isFree;
}

/**
 * Напечатать сообщение
 */
nkmk.Lock.printBusyMessage = function (message){
	var div = this.busyDiv;
	if(div != null && div.messageArea != null){
		div.messageArea.innerHTML = message;
	}
}
