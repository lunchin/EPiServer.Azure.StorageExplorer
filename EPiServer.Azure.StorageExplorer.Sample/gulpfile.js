/// <binding AfterBuild='copy-modules' />
"use strict";
var gulp = require('gulp'),
    clean = require('gulp-clean');

var paths = {
    moduleProtectedRoot: "./modules/_protected/",
    storageExplorer: {
        root: "../EPiServer.Azure.StorageExplorer/",
        views: "../EPiServer.Azure.StorageExplorer/views/",
        clientResources: "../EPiServer.Azure.StorageExplorer/ClientResources/",
        protectedModule: "./modules/_protected/EPiServer.Azure.StorageExplorer/"
    }
};

gulp.task('clean-module',
    function () {
        return gulp.src([paths.storageExplorer.protectedModule + '*'], { read: false })
            .pipe(clean());
    });
gulp.task('copy-static',
    function () {
        gulp.src([paths.storageExplorer.views + '**/*.*']).pipe(gulp.dest(paths.storageExplorer.protectedModule + 'Views/'));
        gulp.src([paths.storageExplorer.root + 'module.config']).pipe(gulp.dest(paths.storageExplorer.protectedModule));
        gulp.src([paths.storageExplorer.clientResources + '**/*.*']).pipe(gulp.dest(paths.storageExplorer.protectedModule + '1.0.1.0/ClientResources/'));
    });
gulp.task('copy-modules', gulp.series('clean-module', 'copy-static'));

