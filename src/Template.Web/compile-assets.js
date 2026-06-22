const fs = require('fs');
const path = require('path');
const sass = require('sass');

const webDir = path.join(__dirname);
const cssDir = path.join(webDir, 'wwwroot', 'css');
const nodeModulesDir = path.join(webDir, 'node_modules');

try {
    console.log('Starting assets compilation and bundling...');

    // 1. Compile SASS (site.scss -> site.css)
    console.log('Compiling site.scss...');
    const result = sass.compile(path.join(cssDir, 'site.scss'), {
        loadPaths: [nodeModulesDir]
    });
    fs.writeFileSync(path.join(cssDir, 'site.css'), result.css);
    console.log('site.css successfully written.');

    // 2. Bundle global CSS (toastify.css + site.css)
    console.log('Bundling global CSS...');
    const toastifyCssPath = path.join(nodeModulesDir, 'toastify-js', 'src', 'toastify.css');
    let toastifyCss = '';
    if (fs.existsSync(toastifyCssPath)) {
        toastifyCss = fs.readFileSync(toastifyCssPath, 'utf8');
    } else {
        console.warn('Warning: toastify.css not found at ' + toastifyCssPath);
    }

    const siteCss = result.css;
    const bundledCss = toastifyCss + '\n' + siteCss;

    fs.writeFileSync(path.join(cssDir, 'bundle-global.css'), bundledCss);
    fs.writeFileSync(path.join(cssDir, 'bundle-global.min.css'), bundledCss); // In dev/test, identical content is fine
    console.log('bundle-global.css and bundle-global.min.css successfully written.');

    console.log('Assets build completed successfully!');
} catch (error) {
    console.error('Error compiling assets:', error);
    process.exit(1);
}
