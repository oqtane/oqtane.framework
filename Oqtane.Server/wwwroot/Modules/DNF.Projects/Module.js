var DNF = DNF || {};

DNF.Projects = {
    createChart: async function (divid, type, labels, datasets, options) {
        var container = document.getElementById(divid);
        if (container.hasChildNodes()) {
            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
        }
        var canvas = document.createElement('canvas');
        canvas.id = divid + '-canvas';
        container.appendChild(canvas);
        var ctx = canvas.getContext('2d');
        var chart = new Chart(ctx, {
            type: type,
            data: {
                labels: labels,
                datasets: datasets
            },
            options: options
        });
    }
};
