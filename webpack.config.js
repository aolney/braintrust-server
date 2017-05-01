var path = require("path");

function resolve(filePath) {
  return path.join(__dirname, filePath);
}
module.exports = {
  devtool: "source-map",
  entry: {
    bundle: resolve("./Server.fsproj")
  },
  output: {
    filename: "[name].js",
    path: resolve("./app/js"),
    libraryTarget: "commonjs2"
  },
  devServer: {
    contentBase: resolve("./app")
  },
  externals: [
       /^(?!\.|\/).+/i,
  ],
  target: "node",
  node: {
    __dirname: false,
    __filename: false
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader"
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules[\\\/](?!fable-)/,
        use: {
          loader: "babel-loader"
        }
      }
    ]
  }
};