module.exports = {
  entry: {
    server: "./temp/server",
  },
  output: {
    filename: "[name].js",
    path: "./app/js",
    libraryTarget: "commonjs2"
  },
    externals: [
        /^(?!\.|\/).+/i,
    ],
  target: "node",
  node: {
    __dirname: false,
    __filename: false
  },
  devtool: "source-map",
  module: {
    preLoaders: [{
      loader: "source-map-loader",
      exclude: /node_modules/,
      test: /\.js$/
    }]
  }
};
