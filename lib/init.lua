AddEventHandler('onServerResourceStart', function(resource)
    if resource == "postgres-async" then
        exports['postgres-async']:psql_configure()
        Citizen.CreateThread(function()
            Citizen.Wait(0)
            TriggerEvent('onPostgresReady')
        end)
    end
end)
