/**
 * Lock.js - ������ ��� ���������� ��������.
 *
 * ������ ������������� ����������� ��� ������������ ��������� ��
 * ����� ���������� ��������. ������� ��� ������ � ajax, �����
 * �������� �� ������������� �� ����� ��������. ����� ���� �����
 * ������� � ��� ����� ���������� � ��������, ������ ��� ��� �������
 * �� ������ ��� ������ ��������� �� �������� � ������������ �����
 * ������ ������ ��� �����-������ ������.
 *
 * |----------------------|
 * | ������ �������������:|
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
 * �� 10 ������ ����������� ��������� � ������ ������� ����� ��������
 * ��������� � ���������� ���������� ������.
 */


/* �������� "������������ ����":
 * ������������ ������������ ���������� ���������� */
var nkmk;

if (!nkmk) {//���� ����� ������ �� ����������
	nkmk = {};//�� ������ ����� ������ ������
} else if (typeof nkmk != "object") {//���� ��� ���������� ���������� ���������� nkmk � ��� �� �������� �������
	throw new Error("nkmk already exists and is not an object");//������ ������
}

if (!nkmk.Lock) {//���� ����� ������ �� ����������
	nkmk.Lock = {};//�� ������ ����� ������ ������
}else if (typeof nkmk.Lock != "object"){//���� ������ ���� ���������� � �� �������� ��������
	throw new Error("nkmk.Lock already exists and is not an object");//������ ������
}

/**
 * ����, ������������ ����������� �� �����-������ �������� (����
 * false, �� �����������).  �������, ��� ������� ����������
 * ������������ ��������� �������, ������ ��������� �������� ������
 * ����������, ��������� ����� nkmk.Lock.isFree() � ������
 * �����������, ������ ���� ���������� ����� true
 */

nkmk.Lock.__isFree = true;

/**
 * ���������� ����������. ������ ������� ������ ���������� �� ���
 * ��������, ������� ����� ���������� ����� ����� ��������
 * if(nkmk.Lock.isFree())
 * @param style (optional) - ����� ����������
 */
nkmk.Lock.setBusy = function(/*optional*/style){
	this.__isFree = false;

	if(style != null){
		if(style == "simple"){
			/* ������ ���� ������ � ������� ������ �� �������� �� ����� ���������� ��������*/
			document.onmousedown = function(){return false;};//�������� ������
			document.onkeydown = function(){return false;};//�������� �������
//			nkmk.Lock.__style = "simple";
		}
	}else{



		document.body.style.cursor = "progress"; //������ ������ �� ������

		/* ������ ���� ������ � ������� ������ �� �������� �� ����� ���������� ��������*/
		document.onmousedown = function(){return false;};//�������� ������
		document.onkeydown = function(){return false;};//�������� �������

		//iframe ����� ���������, ����� ��������� �������� <select>
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

		//div ������� ���, ���������� �������� � ������� ��� ���������
		var div = document.createElement("div");
		div.style.position = "absolute";
		div.style.left = "0";
		div.style.top = "0";
		div.style.width = document.body.scrollWidth;
		div.style.height = document.body.scrollHeight;
		div.style.zIndex = "10051";
		div.align = "center";
		div.innerHTML = "<img style='position: absolute; top: " + document.body.clientHeight / 2 + "' src='Content/Images/process_animation.gif'/>";

		//������� ��� ���������
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

		nkmk.Lock.busyIFrame = iframe;//�������� busyIFrame ������������� ������ �� iframe, ����� �� �������������� � id
	  //� �� �������� �� ����������� id, � ��� ����� ������ � ������ �������
		nkmk.Lock.busyDiv = div;//���� �����
		div.messageArea = innerDiv;

		nkmk.Base.insertFirstChild(document.body, iframe);
		nkmk.Base.insertFirstChild(document.body, div);
		nkmk.Base.insertFirstChild(div, innerDiv);
	}
}

/**
 * ����� ����������. ������ ������� ������ ���������� �� ���
 * ��������, ������� ����� ���������� ����� ����� ��������� ��
 * ����������.  ���� ������� ������ submit ����� ��� ������ ��������
 * document.location, �� ����� �� ����� �� �������� ������
 * �������. ��������� �� ��������� �������� document.location.
 */
nkmk.Lock.setFree = function (){
	var iframe = nkmk.Lock.busyIFrame;
	var div = nkmk.Lock.busyDiv;
	if(div != null){
		div.parentNode.removeChild(div);//������� div �� ���������
		nkmk.Lock.busyDiv = null;
	}
	if(iframe != null){
		iframe.parentNode.removeChild(iframe);//������� frame �� ���������
		nkmk.Lock.busyIFrame = null;
	}
	this.__isFree = true;
	document.onmousedown = function(){};//��������� �����
	document.onkeydown = function(){};//��������� ������� ������
  document.body.style.cursor = "default";//������ ������� ������
}

/**
 * ���������� �� ���������� �� ����������, ���� ���������� ���
 * ���������� true
 */
nkmk.Lock.isFree = function (){
	return this.__isFree;
}

/**
 * ���������� ���������
 */
nkmk.Lock.printBusyMessage = function (message){
	var div = this.busyDiv;
	if(div != null && div.messageArea != null){
		div.messageArea.innerHTML = message;
	}
}
