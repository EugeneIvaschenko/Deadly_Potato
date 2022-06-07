var Platform = {
     IsMobile: function()
     {
         return Module.SystemInfo.mobile;
     }
 };
 
 mergeInto(LibraryManager.library, Platform);