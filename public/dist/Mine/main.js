window.addEventListener("load",init);
let notificator;
function init(){
    notificator=document.getElementById("notificator");
    ShowNotification("Esperando al servidor...");
}
function ShowNotification(message){
    notificator.classList.add("notify");
    notificator.children[0].innerHTML=message;
    setTimeout(StopNotifing,2000);
}
function StopNotifing(){
    notificator.classList.remove("notify");
}

var gameInstance = UnityLoader.instantiate("gameContainer", "Build/WEBGL BUILD.json", {onProgress: UnityProgress});
function InitGame(){
  gameInstance.SendMessage("NetworkSettings","setIp","192.168.0.2");
  gameInstance.SendMessage("SceneLoader","LoadSceneAsync","SampleScene");
}