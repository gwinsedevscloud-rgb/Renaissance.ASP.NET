const {
    setInferVersion,
    withModuleFederationPlugin,
    share
} = require('@angular-architects/module-federation/webpack');
setInferVersion(true);

module.exports = withModuleFederationPlugin({
    name: '_85fdc14b_9a1d_4480_adff_4f1a98971a0c',
    //This section relates directly to the webRemote section of plugin.yml
    //The key is the exposedName for components or name for modules and the value is the path to the
    //respective Angular component or module in your web project. For other web frameworks, it is
    //usually the path to the web element exposing your component
    exposes: {
        'TutorialRouting': './src/main/webapp/app/tutorial/tutorial.routing.ts',
        'ClientDashboardRouting': './src/main/webapp/app/tutorial/components/client-dashboard/client-dashboard.routing.ts'
    },
    shared: share({
        '@angular/core': {singleton: true, strictVersion: false},
        '@angular/common': {singleton: true, strictVersion: false},
        '@angular/common/http': {singleton: true, strictVersion: false},
        '@angular/router': {singleton: true, strictVersion: false},
        '@angular/forms': {singleton: true, strictVersion: false},
        '@angular/animations': {singleton: true, strictVersion: false},
        '@mattae/angular-shared': {singleton: true, strictVersion: false},
        '@angular/material': {singleton: true, includeSecondaries: true, strictVersion: false},
        '@jsverse/transloco': {singleton: true, strictVersion: false},
        'rxjs': {singleton: true, strictVersion: false}
    })
});

