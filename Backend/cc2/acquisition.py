"""Read-only development guidance from authoritative inventory and active catalogs.

This never grants items, changes eligibility, or substitutes for domain commands.
Paid routes are absent until their real fulfillment capability is available.
"""

def development_sources(config, state, business_id, at, quote):
    definition = next((b for b in config['Businesses'] if b['Id'] == business_id), None)
    if definition is None:
        raise KeyError(business_id)
    clears = {int(r['CommandId'].split(':')[1]) for r in state['Economy']['Receipts']
              if r['CommandId'].startswith('certified-first-clear:')}
    requirements = []
    if quote['nextLevel'] is not None:
        for cost in quote['requirements']:
            item, quantity = cost['itemId'], cost['required']
            missing = cost['missing']
            routes = []
            if missing:
                if item in ('CASH', 'WOOD') and len(clears) < 72:
                    routes.append({'kind': 'certified-puzzles', 'free': True,
                                   'available': True, 'repeatable': False,
                                   'explanation': 'Earn first-clear rewards in unfinished free cities.'})
                for producer in config['Businesses']:
                    if producer['Output'] != item:
                        continue
                    installation = next((b for b in state['Businesses'] if b['Id'] == producer['Id'] and b['Level'] > 0), None)
                    routes.append({'kind': 'production', 'businessId': producer['Id'],
                                   'free': True, 'repeatable': True,
                                   'available': installation is not None,
                                   'unlockClears': producer['UnlockClears'],
                                   'explanation': 'Collect bounded output from this completed property.' if installation else 'Develop this property to produce the material.'})
                active = sum(not d['Claimed'] for d in state.get('Dispatches', []))
                slots = 2 if len(clears) >= config['Operations']['SecondFreeDispatchClears'] else 1
                today = sum(d['StartedAt'] // 86400 == at // 86400 for d in state.get('Dispatches', []))
                for dispatch in config['Operations']['Dispatches']:
                    if not any(r['ItemId'] == item for r in dispatch['Rewards']):
                        continue
                    eligible = any(b['Level'] > 0 for b in state['Businesses']) and len(clears) >= dispatch['UnlockClears']
                    routes.append({'kind': 'dispatch', 'templateId': dispatch['Id'],
                                   'free': True, 'repeatable': True,
                                   'unlocked': eligible, 'capacityAvailable': active < slots and today < config['Operations']['DailyDispatchStarts'],
                                   'availability': 'requires-verified-plan-and-fee',
                                   'costs': dispatch['Costs'], 'rewards': dispatch['Rewards'],
                                   'seconds': dispatch['Seconds'],
                                   'requiresSavedContributingRecipe': dispatch['RequiredRecipe'],
                                   'explanation': 'A completed qualifying plan and the listed dispatch fee are required.'})
            requirements.append({'itemId': item, 'required': quantity,
                                 'owned': cost['owned'], 'missing': missing,
                                 'sources': routes})
    return {**quote, 'requirements': requirements,
            'paidRoutes': [], 'paidCommerceEnabled': False,
            'note': 'Costs,shortages,rates and action come from the shared C# domain. Source definitions are advisory; every development command is revalidated.'}
