/**
 * Base.js модуль содержит общие функции
 */

/* Создание "пространства имен":
 * Определяется единственная глобальная переменная */
var nkmk;

if (!nkmk) {//если такой объект не существует
	nkmk = {};//то создаём новый пустой объект
} else if (typeof nkmk != "object") {//если уже определена глобальная переменная nkmk и она не является объктом
	throw new Error("nkmk already exists and is not an object");//выдать ошибку
}

if (!nkmk.Base) {//если такой объект не существует
	nkmk.Base = {};//то создаём новый пустой объект
}else if (typeof nkmk.Base != "object"){//если данное поле определено и не является объектом
	throw new Error("nkmk.Base already exists and is not an object");//выдать ошибку
}


/**
 * Удаляет пробелы на концах строки.
 * Пример использования: " 1 ".trim() -> "1"
 */
String.prototype.trim = function(){
	return this.replace(/^\s/gi, "").replace(/\s$/gi, "");
}

/**
 * Вставить перед первым потомком данного элемента-контейнера
 * @param container элемент контейнер
 * @param element вставляемый элемент 
 */
nkmk.Base.insertFirstChild = function(container, element){
  container.insertBefore(element, container.firstChild);
}

/**
 * Получить удобное строковое предствление объекта
 */
nkmk.Base.object2string = function(o){
	var s = "";
	for(i in o){
		s += "['" + i + "'] = '" + o[i] + "'\n";
	}
	return s;
}


/**
 * Отменить disabled для всех текстовых полей формы. Нужно потому, что disabled-поля не
 * передаются на сервер при submit'е формы.
 *
 * @param formId идентификатор формы
 */
nkmk.Base.enableDisabledFields = function(formId){
  var form = $(formId);
  for (i = 0; i < form.elements.length; i++) {
    if (form.elements[i].type == "text") 
      form.elements[i].disabled = false;
  }
}


/**
 * Показать ошибку. Если ошибка строка, покажет строку, иначе покажет все поля ошибки как
 * объекта.
 * 
 * @param error ошибка
 */
nkmk.Base.showError = function (error){
  var s = "";
  if(typeof error == "string"){
    s = error;
  }else{
    for(j in error){
      s += j + " = " + error[j] + "\r\n";
    }
  }
  alert(s);    
  //если окно заблокировано, разблокировать его:
  if(!nkmk.Lock.isFree()){
    nkmk.Lock.setFree();
  }
}