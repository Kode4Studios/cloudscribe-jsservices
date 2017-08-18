import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component
export default class NavMenuComponent extends Vue {
    routeToTenant(path:string) {
        return (Vue as any).getTenantRoute(path);
    }
}