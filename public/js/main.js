window.addEventListener("load",function(){
    var btn=document.getElementById("btnIngresar");

    btn.onclick=function(){
        var name=document.getElementById("inputName").value;
        btn.setAttribute("href","/dist/"+ name);
    };

});
window.addEventListener("keydown",function(e){

    if(e.keyCode!=13){return;}
    var btn=document.getElementById("btnIngresar");
    var name=document.getElementById("inputName").value;
    btn.setAttribute("href","/dist/"+ name);
    btn.click();
});

