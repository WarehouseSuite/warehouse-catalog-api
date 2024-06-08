using CatalogApplication.Database;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.ReplyTypes;

namespace CatalogApplication.Seed;

internal static class ProductSeeder
{
    const int LoopSafety = 1000;
    const int ProductsPerPrimaryCategory = 100;
    
    internal static async Task<Reply<bool>> SeedProducts( IDapperContext dapper, List<Category> categories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        return Reply<bool>.None();
    }
    static List<Product> GenerateProducts( IDapperContext dapper, List<Category> primaryCategories, Dictionary<Guid, List<Category>> secondaryCategoriesByPrimaryId, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        List<Product> products = [];
        List<ProductCategory> productCategories = [];
        List<ProductDescription> productDescriptions = [];

        foreach ( Category primaryCategory in primaryCategories ) {
            for ( int i = 0; i < ProductsPerPrimaryCategory; i++ ) {
                List<Category> selectedCategories = PickRandomCategories( primaryCategory, secondaryCategoriesByPrimaryId, random );
                Product p = new(
                    Guid.NewGuid(),
                    primaryCategory.Id,
                    PickBrandId( brands, random ),
                    PickRating( random ),
                    PickIsInStock( random ),
                    PickIsFeatured( random ),
                    PickName( primaryCategory, i ),
                    PickImage( primaryCategories, random ),
                    PickPrice( random, out decimal price ),
                    PickSalePrice( price, random ) );
                List<ProductCategory> pc = GenerateProductCategories( p, selectedCategories );
                
                products.Add( p );
                productCategories.AddRange( pc );
            }
        }
        
        return products;
    }
    static List<Category> PickRandomCategories( Category primaryCategory, Dictionary<Guid, List<Category>> secondaryCategoriesByPrimaryId, RandomUtility random )
    {
        List<Category> selectedSecondary = [];
        int numSecondary = random.GetRandomInt( 1, 4 );
        for ( int i = 0; i < numSecondary; i++ ) {
            for ( int j = 0; j < LoopSafety; j++ ) {
                List<Category> subCategories = secondaryCategoriesByPrimaryId[primaryCategory.Id];
                int index = random.GetRandomInt( subCategories.Count - 1 );
                Category currentCategory = subCategories[index];
                if (selectedSecondary.Contains( currentCategory ))
                    continue;
                selectedSecondary.Add( currentCategory );
                break;
            }
        }
        
        List<Category> selectedCategories = [primaryCategory];
        selectedCategories.AddRange( selectedSecondary );
        return selectedCategories;
    }
    static List<ProductCategory> GenerateProductCategories( Product p, List<Category> selectedCategories )
    {
        List<ProductCategory> pc = [];
        foreach ( Category c in selectedCategories )
            pc.Add( new ProductCategory( p.Id, c.Id ) );
        return pc;
    }
    static Guid PickBrandId( List<Brand> brands, RandomUtility random )
    {
        int index = random.GetRandomInt( brands.Count - 1 );
        return brands[index].Id;
    }
    static float PickRating( RandomUtility random )
    {
        float value = (float) random.GetRandomDouble( Consts.MaxRating );
        return value;
    }
    static bool PickIsInStock( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.95 );
        return value;
    }
    static bool PickIsFeatured( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.2 );
        return value;
    }
    static string PickName( Category primaryCategory, int iteration )
    {
        string pName = SeedData.PrimaryCategories[primaryCategory.Name];
        string name = $"{pName} {iteration}";
        return name;
    }
    static string PickImage( List<Category> primaryCategories, RandomUtility random )
    {
        int cIndex = random.GetRandomInt( primaryCategories.Count - 1 );
        List<string> images = SeedData.ProductImagesByPrimaryCategory[primaryCategories[cIndex].Name];
        int iIndex = random.GetRandomInt( images.Count - 1 );
        return images[iIndex];
    }
    static decimal PickPrice( RandomUtility random, out decimal price )
    {
        price = (decimal) random.GetRandomDouble( SeedData.MaxPrice );
        return price;
    }
    static decimal PickSalePrice( decimal mainPrice, RandomUtility random )
    {
        bool hasSale = random.GetRandomBool( 0.2 );
        if (!hasSale)
            return decimal.Zero;

        double maxSalePrice = 0.9 * (double) mainPrice;
        double salePrice = random.GetRandomDouble( maxSalePrice );
        return (decimal) salePrice;
    }
}