var Oqtane = Oqtane || {};
Oqtane.Controls = Oqtane.Controls || {};

//interop functions for tabstrip control
Oqtane.Controls.TabStrip = Oqtane.Controls.TabStrip || {};
Oqtane.Controls.TabStrip.Interop = {
    updateView: function (id) {
        var activeTab = document.querySelector('.nav-tabs .nav-item a.active[href^="#' + id + '"]');
        if (activeTab != null) {
            var tabPanel = document.getElementById(activeTab.getAttribute("href").replace("#", ""));
            if (tabPanel != null && !tabPanel.classList.contains("active")) {
                document.querySelectorAll('div[id^="' + id + '"]').forEach(i => i.classList.remove('show', 'active'));
                tabPanel.classList.add('show', 'active');
            }
        }
    }
};
