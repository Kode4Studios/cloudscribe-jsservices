import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import MultiTenantRouterPlugin from './plugins/multi-tenant-router';
Vue.use(VueRouter);
Vue.use(MultiTenantRouterPlugin, {
    mode: "filepath",
    appRoot: ["home", "spa"]
});

const appPath = "/home/spa";
var appPathIdx = document.location.href.indexOf(appPath);
var tenantIndx = document.location.href.indexOf("/home");

const tenantPathMatch = /^http[s]?:\/\/.*?\/([a-zA-Z-_]+).*$/;
let parts = document.location.href.match(tenantPathMatch);   


let tenant = "/";
if (appPathIdx !== tenantIndx
    || (parts&&parts.length>1&&parts[1]!=="home")) {
    tenant = "/"+parts[1]+"/";
}

console.log(tenant);

const getRoute = (path:string) => {
    return (Vue as any).getTenantRoute(path);
};

const routes = [
    { path: `${getRoute("home/spa/")}`, component: require('./components/home/home.vue.html') },
    { path: `${getRoute("home/spa/counter")}`, component: require('./components/counter/counter.vue.html') },
    { path: `${getRoute("home/spa/fetchdata")}`, component: require('./components/fetchdata/fetchdata.vue.html') }
];

new Vue({
    el: '#app-root',
    router: new VueRouter({ mode: 'history', routes: routes }),
    render: h => h(require('./components/app/app.vue.html'))
});
