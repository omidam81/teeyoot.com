﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Teeyoot.WizardSettings.ViewModels
{
    public class ProductViewModel
    {
        public ProductViewModel() : this(null)
        {
        }

        public ProductViewModel(int? productId)
        {
            Id = productId;

            ProductColours = new List<ProductColourItemViewModel>();
            SelectedProductColours = new List<ProductColourItemViewModel>();

            ProductGroups = new List<ProductGroupItemViewModel>();
            SelectedProductGroups = new List<int>();

            SelectedProductSizes = new List<int>();
        }

        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Materials { get; set; }
        public string Details { get; set; }
        public float BaseCost { get; set; }
        public float PackagingCost { get; set; }

        public IEnumerable<ProductColourItemViewModel> ProductColours { get; set; }
        public List<ProductColourItemViewModel> SelectedProductColours { get; set; }
        public IEnumerable<ProductGroupItemViewModel> ProductGroups { get; set; }
        public IEnumerable<int> SelectedProductGroups { get; set; }
        public IEnumerable<ProductHeadlineViewModel> ProductHeadlines { get; set; }
        public IEnumerable<ProductSizeItemViewModel> ProductSizes { get; set; }
        public IEnumerable<int> SelectedProductSizes { get; set; }
        public IEnumerable<float> SelectedProductSizesCost { get; set; }

        [Required]
        public int SelectedProductHeadline { get; set; }

        public HttpPostedFileBase ProductImageFront { get; set; }
        public HttpPostedFileBase ProductImageBack { get; set; }
        public string ProductImageFrontFileName { get; set; }
        public string ProductImageBackFileName { get; set; }
        public int xOrder { get; set; }

    }
}