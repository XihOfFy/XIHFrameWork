var WXTouchLibrary = 
{
  $WXTouchManager: 
  {
    onTouchMove: null,
  }, 

  WX_RegisterOnTouchMoveCallback: function (callback) {
    Module["WXTouchManager"] = WXTouchManager;
    WXTouchManager.onTouchMove = callback;
  },
};

autoAddDeps(WXTouchLibrary, '$WXTouchManager');
mergeInto(LibraryManager.library, WXTouchLibrary);