/**
 * Base.js ������ �������� ����� �������
 */

/* �������� "������������ ����":
 * ������������ ������������ ���������� ���������� */
var nkmk;

if (!nkmk) {//���� ����� ������ �� ����������
	nkmk = {};//�� ������ ����� ������ ������
} else if (typeof nkmk != "object") {//���� ��� ���������� ���������� ���������� nkmk � ��� �� �������� �������
	throw new Error("nkmk already exists and is not an object");//������ ������
}

if (!nkmk.Base) {//���� ����� ������ �� ����������
	nkmk.Base = {};//�� ������ ����� ������ ������
}else if (typeof nkmk.Base != "object"){//���� ������ ���� ���������� � �� �������� ��������
	throw new Error("nkmk.Base already exists and is not an object");//������ ������
}


/**
 * ������� ������� �� ������ ������.
 * ������ �������������: " 1 ".trim() -> "1"
 */
String.prototype.trim = function(){
	return this.replace(/^\s/gi, "").replace(/\s$/gi, "");
}

/**
 * �������� ����� ������ �������� ������� ��������-����������
 * @param container ������� ���������
 * @param element ����������� ������� 
 */
nkmk.Base.insertFirstChild = function(container, element){
  container.insertBefore(element, container.firstChild);
}

/**
 * �������� ������� ��������� ������������ �������
 */
nkmk.Base.object2string = function(o){
	var s = "";
	for(i in o){
		s += "['" + i + "'] = '" + o[i] + "'\n";
	}
	return s;
}


/**
 * �������� disabled ��� ���� ��������� ����� �����. ����� ������, ��� disabled-���� ��
 * ���������� �� ������ ��� submit'� �����.
 *
 * @param formId ������������� �����
 */
nkmk.Base.enableDisabledFields = function(formId){
  var form = $(formId);
  for (i = 0; i < form.elements.length; i++) {
    if (form.elements[i].type == "text") 
      form.elements[i].disabled = false;
  }
}


/**
 * �������� ������. ���� ������ ������, ������� ������, ����� ������� ��� ���� ������ ���
 * �������.
 * 
 * @param error ������
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
  //���� ���� �������������, �������������� ���:
  if(!nkmk.Lock.isFree()){
    nkmk.Lock.setFree();
  }
}