(function($) {
    "use strict";
    
    function getTime() {
        //Credits to Raj: http://stackoverflow.com/a/12612778/232175
        return new Date().toTimeString().replace(/.*(\d{2}:\d{2}:\d{2}).*/, "$1");
    }
    
    $.fn.clock = function() {
        var $this = $(this);
        var tick = function() {
            //Stop all animations
            $this.stop(true, true);

            //Refresh time
            var time = getTime();
            $this.text(time);
    
            //Fadein + fadeout
            $this.fadeIn(500, function() {
                $this.fadeOut(500);
            });            
        }
        
        //Hide clock initially
        $this.fadeOut(0);
        //Start timer
        setInterval(tick, 1000);
        //Do first tick now
        tick();
    }
}(jQuery));