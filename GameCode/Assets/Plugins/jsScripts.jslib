mergeInto(LibraryManager.library, {
    BrowserNotification:function(message){
       ShowNotification(message.toString());         
    },
    InitBrowserConf:function(){
        InitGame();
    }
}); 