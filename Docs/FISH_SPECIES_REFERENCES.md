# FISH! Philippine species references

Research checked 2026-11-11. Exact scientific names identify entries; vernacular names are not unique species identifiers. No Negros-specific name is asserted. Gameplay rarity, weights, stats and abilities are fictional and unchanged. Images/models remain placeholders; no assets downloaded. Existing placeholder-named asset paths and species IDs are intentionally retained for reference/save compatibility.

Each numbered record maps directly to one of the five catch catalog entries. URLs below identify occurrence, naming, and biology/description evidence. Text is paraphrased; no claim that these five are equally common or have the same conservation status.

## Chanos chanos — Milkfish

Asset: `Assets/Data/Fish/PlaceholderFish.asset`. Local name: **Bangus**.
Tagalog, Philippines; Negros-specific usage not established.

Habitat: Coastal bays and marine waters; juveniles also use mangroves and estuaries.
Description: A streamlined fish with silvery sides, a small toothless mouth and a deeply forked tail.
Educational fact: Milkfish mature at sea and spawn in fully saline water.

[Occurrence / habitat reference](https://www.fishbase.se/Country/CountrySpeciesSummary.php?Country=Philippines&genusname=Chanos&speciesname=chanos)
[Local-name reference](https://www.fishbase.se/Aquaculture/AquacultureProfileSummary.php?GenusName=Chanos&ID=80&SpeciesName=chanos)
[Biology / description reference](https://www.fishbase.org/summary/chanos-chanos)

## Sillago sihama — Silver sillago

Asset: `Assets/Data/Fish/CatchPlaceholder2.asset`. Local name: **Asohos**.
Tagalog, Philippines; also documented in other Philippine languages. Not unique to this species.

Habitat: Shallow beaches, sandbars, mangrove creeks and estuaries.
Description: An elongated coastal fish with a pale, silvery underside.
Educational fact: Adults can bury themselves in sand when disturbed.

[Occurrence / habitat reference](https://www.fishbase.se/Country/CountrySpeciesSummary.php?Country=Philippines&genusname=Sillago&speciesname=sihama)
[Local-name reference](https://fishbase.se/ComNames/CommonNameSearchList.php?CommonName=Asohos)
[Biology / description reference](https://www.fao.org/4/x2400e/x2400e48.pdf)

## Siganus guttatus — Orange-spotted spinefoot

Asset: `Assets/Data/Fish/CatchPlaceholder3.asset`. Local name: **Samaral**.
Hiligaynon, Philippines, as listed by FishBase; no Negros-specific locality verified. Shared vernacular name.

Habitat: Shallow coastal waters, including seagrass and mangrove areas with brackish water.
Description: An oval, flattened rabbitfish with orange to bronze spots and a bright yellow patch near the rear dorsal fin.
Educational fact: This species feeds on algae growing on the bottom.

[Occurrence / habitat reference](https://www.kahaku.go.jp/research/db/zoology/Fishes_of_Andaman_Sea/contents/siganidae/02.html)
[Local-name reference](https://www.fishbase.se/comnames/CommonNamesList.php?GenusName=Siganus&ID=4588&SpeciesName=guttatus&StockCode=4774)
[Biology / description reference](https://www.fishbase.se/summary/Siganus-guttatus.html)

## Scatophagus argus — Spotted scat

Asset: `Assets/Data/Fish/CatchPlaceholder4.asset`. Local name: **Kitang**.
Tagalog, Philippines; Negros-specific usage not established. Vernacular names are not unique identifiers.

Habitat: Sheltered bays, harbors and brackish estuaries, often near mangroves.
Description: A deep-bodied, flattened fish with dark spots that can become faint in large adults.
Educational fact: Its varied diet includes worms, crustaceans, insects and plant material.

[Occurrence / habitat reference](https://www.fishbase.se/country/CountrySpeciesSummary.php?Country=Philippines&genusname=Scatophagus&speciesname=argus)
[Local-name reference](https://fishbase.se/ComNames/CommonNamesList.php?GenusName=Scatophagus&ID=4698&SpeciesName=argus&StockCode=4919)
[Biology / description reference](https://www.fao.org/4/w6598e/W6598E06.htm)

## Lutjanus argentimaculatus — Mangrove red snapper

Asset: `Assets/Data/Fish/CatchPlaceholder5.asset`. Local name: **Mangagat**.
Philippine usage documented by NFRDI; language and Negros-specific usage not specified in that source.

Habitat: Young fish use mangrove estuaries and tidal creeks; adults move to deeper reef areas.
Description: A reddish-brown coastal snapper that preys on fish and crustaceans.
Educational fact: Mangrove red snappers feed mainly at night.

[Occurrence / habitat reference](https://fishbase.se/country/CountrySpeciesSummary.php?Country=Philippines&genusname=Lutjanus&lang=french&speciesname=argentimaculatus)
[Local-name reference](https://nfrdi.da.gov.ph/tpjf/etc/Common%20and%20Local%20Names%20of%20Marine%20Fishes%20of%20the%20Phils.PDF)
[Biology / description reference](https://www.ciesm.org/atlas_preview/Lutjanusargentimaculatus.php)

## Evidence limits and unresolved questions

- Asohos is explicitly listed for several sillaginids: never identify a specimen by that market name alone. Sillago sihama has documented look-alike identification issues; source images should be checked carefully before replacing placeholder art.
- Samaral is a shared vernacular name. FishBase explicitly supports Hiligaynon for Siganus guttatus, but does not establish a Negros locality in the cited name row. Orange-spotted spinefoot follows the museum/FAO usage; FishBase uses Goldlined spinefoot as its heading. Keep the exact binomial to avoid confusion with Siganus lineatus.
- Mangagat is supported by the NFRDI scientific-name/English/local-name table. Its PDF returned a fetch error, but the indexed table exposed the exact row. Language remains unspecified; do not relabel it Hiligaynon without further evidence.
- Kitang and Bangus use documented Philippine/Tagalog usage; this is not a claim about names preferred in every Negros community. A local fisheries specialist should review regional wording before final localization.
- No matching images/models were acquired. The visual fallback remains intentional. Encounter enemies and environmental boss remain fictional fixtures.

## Verification

Five existing assets populated in place; a whitelist comparison confirmed no changes outside content fields and isPlaceholder. Existing GUID files, IDs, catalog order/weights and battle fields were left untouched. Encyclopedia code reads these shared names, habitat, description, fact and URLs and handles absent icons; static display-path inspection only. Unity import, wrapping/scrolling, visible entry unlocking and local-language display still require manual testing. Let Unity refresh, then open discovered entries with N; no setup command required. Repeat 5A setup reuses existing assets and preserves content.
