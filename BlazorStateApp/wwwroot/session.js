// Session persistence for blue-green deployments
window.blazorSession = {
    getSessionId: function () {
        let sessionId = localStorage.getItem('blazor-session-id');
        if (!sessionId) {
            sessionId = this.generateGuid();
            localStorage.setItem('blazor-session-id', sessionId);
        }
        return sessionId;
    },
    
    clearSessionId: function () {
        localStorage.removeItem('blazor-session-id');
    },
    
    generateGuid: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
};
