const { app } = require('@azure/functions');

app.http('AsyncGlitchFunction', {
    methods: ['GET', 'POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        context.log('Function started, but async call is not properly awaited.');

        // Incorrectly calling an async method without awaiting it
        const result = fetchDataFromApi(context);

        context.log('Returning response before async task completes.');
        return { body: `This function returns before the async operation finishes!` };
    }
});


const axios = require('axios');

/**
 * Simulates an async API call that should be awaited.
 */
async function fetchDataFromApi(context) {
    try {
        const response = await axios.get('https://example.com/data');
        context.log('Response:', response.data);
        return response.data;
    } catch (error) {
        context.log('Error:', error.message);
        return null;
    }
}