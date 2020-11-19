module.exports = async function (context, eventGridEvent) {
    context.bindings.cosmos = JSON.stringify({
        id: eventGridEvent.id,
        data: JSON.parse(eventGridEvent.data)
    });
};