module.exports = async function (context) {
    context.res = {
        body: context.bindings.cosmos
    };
}