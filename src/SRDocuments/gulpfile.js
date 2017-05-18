/// <binding BeforeBuild='min:css, min:js' />
var gulp = require('gulp'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    uglify = require('gulp-uglify');

gulp.task('min:css', function () {
    gulp.src("wwwroot/css/site.css")
        .pipe(concat("site.min.css"))
        .pipe(cssmin())
        .pipe(gulp.dest("wwwroot/css"));

    gulp.src("wwwroot/css/sideBar.css")
        .pipe(concat("sideBar.min.css"))
        .pipe(cssmin())
        .pipe(gulp.dest("wwwroot/css"));

    gulp.src("wwwroot/css/login.css")
        .pipe(concat("login.min.css"))
        .pipe(cssmin())
        .pipe(gulp.dest("wwwroot/css"));
});

gulp.task('min:js', function () {
    gulp.src("wwwroot/js/site.js")
        .pipe(concat("site.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("wwwroot/js"));

    gulp.src("wwwroot/js/sideBar.js")
        .pipe(concat("sideBar.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("wwwroot/js"));

    gulp.src("wwwroot/js/login.js")
        .pipe(concat("login.min.js"))
        .pipe(uglify())
        .pipe(gulp.dest("wwwroot/js"));
});

gulp.task('default', function () {

});