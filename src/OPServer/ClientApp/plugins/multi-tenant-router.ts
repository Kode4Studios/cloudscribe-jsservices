import Vue from 'vue';

const MultiTenantRouterPlugin = {
    tenantRouteBase: "/",
    tenantKey: "/",
    install(Vue, opts) {
        let tenant = "/";
        let routeBase = "/";
        const tenantPathMatch = /^http[s]?:\/\/.*?\/([a-zA-Z-_]+).*$/;
        let parts = document.location.href.match(tenantPathMatch);

        if (opts&&opts.mode === "filepath") {
            const appPath = "/" + opts.appRoot.join("/");
            var appPathIdx = document.location.href.indexOf(appPath);
            var tenantIndx = document.location.href.indexOf("/" + opts.appRoot[0]);


            if (appPathIdx !== tenantIndx
                || (parts && parts.length > 1 && parts[1] !== "home")) {
                routeBase = "/" + parts[1] + "/";
                tenant = parts[1];
            }            
        }
        else {
            let dns = document.location.href.match(/^(?:https?:\/\/)?(?:[^@\n]+@)?(?:www\.)?([^:\/\n]+)/im);
            routeBase = dns[1];
        }
        this.tenantRouteBase = routeBase;
        this.tenant = tenant;

        Vue.getTenantRoute = (path) => {
            let tenantPath = `${this.tenantRouteBase}${path}`;
            return tenantPath;
        }
    }
};

export default MultiTenantRouterPlugin;