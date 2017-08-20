import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import MultiTenantRouting from './plugins/multi-tenant-router';
Vue.use(VueRouter);
Vue.use(MultiTenantRouting, {
    mode: "filepath", /* haven't tried or tested dns based tenant identification */
    appRoot: ["home", "spa"] /* slugs in order that make up the path to your spa page - in this case this maps to the url /home/spa */
});

/* When building any route use the multi-tenant routing function attached to the main Vue object */
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
