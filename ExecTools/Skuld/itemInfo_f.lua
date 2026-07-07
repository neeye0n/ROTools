-- Original translation works of zackdreaver: https://github.com/zackdreaver/ROenglishRE
-- Continuated by llchrisll at https://github.com/llchrisll/ROenglishRE
-- This file can be distributed, used and modified freely
-- This file shouldn't be claimed as part of your project, unless you fork it from https://github.com/llchrisll/ROenglishRE
-- Last updated: 20250515

-- Load custom made itemAnnotations.lua
dofile("System/LuaFiles514/itemAnnotations.lua")

local function trim(s)
    return s:match("^%s*(.-)%s*$")
end

local customServerItemDb = "Elegy"

function main()
	for ItemID, DESC in pairs(tbl) do
		
		-- Read and apply suffix to DESC.identifiedDisplayName
		local displayName = DESC.identifiedDisplayName
		local ann = itemAnnotations[ItemID]
		if ann ~= nil then
			if ann.nameTagPrefix ~= nil and ann.nameTagPrefix ~= "" then
				displayName = trim(ann.nameTagPrefix) .. " " .. trim(displayName)
			end
			if ann.nameTagSuffix ~= nil and ann.nameTagSuffix ~= "" then
				displayName = trim(displayName) .. " " .. trim(ann.nameTagSuffix)
			end
		end

		-- START Replace `DESC.identifiedDisplayName` with `displayName` in this region
		if DisplayServer == 1 and DESC.Server ~= nil and DESC.Custom == nil then
			result, msg = AddItem(ItemID, DESC.unidentifiedDisplayName, DESC.unidentifiedResourceName, displayName..' '..TagStart..DESC.Server..TagEnd, DESC.identifiedResourceName, DESC.slotCount, DESC.ClassNum)
		elseif DisplayCustomServer == 1 and DESC.Custom == true then
			result, msg = AddItem(ItemID, DESC.unidentifiedDisplayName, DESC.unidentifiedResourceName, displayName..' '..CustomTagStart..CServerName..CustomTagEnd, DESC.identifiedResourceName, DESC.slotCount, DESC.ClassNum)
		else
			result, msg = AddItem(ItemID, DESC.unidentifiedDisplayName, DESC.unidentifiedResourceName, displayName, DESC.identifiedResourceName, DESC.slotCount, DESC.ClassNum)
		end
		if not result == true then
			return false, msg
		end
		-- END Replace `DESC.identifiedDisplayName` with `displayName` in this region

		-- This exists only for ChangeMaterial.lub file, since Gravity decided to use the unidentifiedDescriptionName
		-- for items, where the player does not have the required amount or none at all.
		-- Thanks to Optimus for the report ;)
		if DESC.unidentifiedDescriptionName[1] == "" then
			for k, v in pairs(DESC.identifiedDescriptionName) do
				result, msg = AddItemUnidentifiedDesc(ItemID, v)
				if not result == true then
					return false, msg
				end
			end
		else 
			for k, v in pairs(DESC.unidentifiedDescriptionName) do
				result, msg = AddItemUnidentifiedDesc(ItemID, v)
				if not result == true then
					return false, msg
				end
			end
		end
		if (DisplayServer == 2 and DESC.Server ~= nil) or (DisplayCustomServer == 2 and DESC.Custom == true) or DisplayItemID == 1 then
			if DisplayServer == 2 and DESC.Server ~= nil then
				AddItemIdentifiedDesc(ItemID, "^0000CCServer: "..ServerColour..DESC.Server.."^000000")
			elseif DisplayCustomServer == 2 and DESC.Custom == true then
				AddItemIdentifiedDesc(ItemID, "^0000CCServer: "..CServerColour..CServerName.."^000000")
			end
			if DisplayItemID == 1 then
				AddItemIdentifiedDesc(ItemID, "^0000CCID:^000000 "..ItemID)
			end

			-- START Read and Apply item annotations
			if itemAnnotations ~= nil and itemAnnotations[ItemID] ~= nil and itemAnnotations[ItemID].descLines ~= nil then
				for _, line in ipairs(itemAnnotations[ItemID].descLines) do
					AddItemIdentifiedDesc(ItemID, line)
				end
			end
			-- END Read and Apply item annotations
			
			AddItemIdentifiedDesc(ItemID, "________________________")
		end
		if RemoveWeight == true then
			for k, v in pairs(DESC.identifiedDescriptionName) do
				if string.find(v, 'Weight:^000000') ~= nil then
					if string.find(tostring(DESC.identifiedDescriptionName[k-1]),'______') ~= nil then
						table.remove(DESC.identifiedDescriptionName,(k-1))
						table.remove(DESC.identifiedDescriptionName,(k-1))
					else
						table.remove(DESC.identifiedDescriptionName,k)
					end
				end
			end
		end
		-- START Print Item ID first before other descriptions
		if (DisplayServer == 3 and DESC.Server ~= nil) or (DisplayCustomServer == 3 and DESC.Custom == true) or DisplayItemID == 2 or DisplayDatabase == true or DisplayCustomDB == true then
			if DisplayDatabase == true or DisplayCustomDB == true then
				if DisplayDatabase == true and DESC.Custom == nil then
					local Database = ItemDatabase[customServerItemDb]
					if DESC.Server ~= nil and ItemDatabase[DESC.Server] ~= nil then
						Database = ItemDatabase[DESC.Server]
					end
				-- START Unify Item ID and Item Url
					-- AddItemIdentifiedDesc(ItemID, "<URL>" .. Database.Name .. "<INFO>" .. Database.URL .. ItemID .. "</INFO></URL>")
					AddItemIdentifiedDesc(ItemID, "^0000CCID:^000000 <URL>" .. ItemID .. "<INFO>" .. Database.URL .. ItemID .. "</INFO></URL>")
				elseif DisplayCustomDB == true and DESC.Custom == true then
					-- AddItemIdentifiedDesc(ItemID, "<URL>" .. ItemDatabase["Custom"].Name .. "<INFO>" .. ItemDatabase["Custom"].URL .. ItemID .. "</INFO></URL>")
					AddItemIdentifiedDesc(ItemID, "^0000CCID:^000000 <URL>" .. ItemID .. "<INFO>" .. ItemDatabase["Custom"].URL .. ItemID .. "</INFO></URL>")
				end
				-- END Unify Item ID and Item Url
			end
		end 
		-- END Print Item ID first before other descriptions

		for k, v in pairs(DESC.identifiedDescriptionName) do
			result, msg = AddItemIdentifiedDesc(ItemID, v)
			if not result == true then
				return false, msg
			end
		end

		if (DisplayServer == 3 and DESC.Server ~= nil) or (DisplayCustomServer == 3 and DESC.Custom == true) or DisplayItemID == 2 or DisplayDatabase == true or DisplayCustomDB == true then
			AddItemIdentifiedDesc(ItemID, "________________________")
			
			-- START Read and Apply item annotations
			if itemAnnotations ~= nil and itemAnnotations[ItemID] ~= nil and itemAnnotations[ItemID].descLines ~= nil then
				for _, line in ipairs(itemAnnotations[ItemID].descLines) do
					AddItemIdentifiedDesc(ItemID, line)
				end
			end
			-- END Read and Apply item annotations

			if DisplayServer == 3 and DESC.Server ~= nil and DESC.Custom == nil then
				AddItemIdentifiedDesc(ItemID, "^0000CCServer: "..ServerColour..DESC.Server.."^000000")
			elseif DisplayCustomServer == 3 and DESC.Custom == true then
				AddItemIdentifiedDesc(ItemID, "^0000CCServer: "..CServerColour..CServerName.."^000000")
			end

		end
		if DESC.EffectID~= nil then
			result, msg = AddItemEffectInfo(ItemID, DESC.EffectID)
			if not result == true then
				return false, msg
			end
		end
		if DESC.costume ~= nil then
			result, msg = AddItemIsCostume(ItemID, DESC.costume)
			if not result == true then
				return false, msg
			end
		end
		if DESC.PackageID ~= nil then
			result = AddItemPackageID(ItemID, DESC.PackageID)
			if not result then
				return false, msg
			end
		end
	end
	return true, "good"
end

function main_server()
	for ItemID, DESC in pairs(tbl) do
		result, msg = AddItem(ItemID, DESC.identifiedDisplayName, DESC.slotCount)
		if not result == true then
			return false, msg
		end
	end
	return true, "good"
end