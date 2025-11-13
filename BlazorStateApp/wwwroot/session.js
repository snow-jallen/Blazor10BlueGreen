// Session persistence for blue-green deployments
// Using sessionStorage instead of localStorage to ensure per-tab isolation
window.blazorSession = {
    getSessionId: function () {
        let sessionId = sessionStorage.getItem('blazor-circuit-id');
        if (!sessionId) {
            sessionId = this.generateGuid();
            sessionStorage.setItem('blazor-circuit-id', sessionId);
        }
        return sessionId;
    },
    
    clearSessionId: function () {
        sessionStorage.removeItem('blazor-circuit-id');
    },
    
    generateGuid: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
};
