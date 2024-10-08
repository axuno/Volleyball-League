﻿using System.ComponentModel.DataAnnotations;
using League.Models.VenueViewModels;
using League.Resources;
using TournamentManager.DAL.EntityClasses;

namespace League.Components;

public class VenueEditorComponentModel
{
    [HiddenInput]
    public long? Id { get; set; }

    [HiddenInput]
    [Required]
    public bool IsNew { get; set; }
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Venue name")]
    public string Name { get; set; } = string.Empty;
        
    [Display(Name = "Playing field")]
    [DataType(DataType.Text)]
    public string Extension { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Street")]
    public string Street { get; set; } = string.Empty;
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Postal code")]
    [StringLength(10, MinimumLength = 4, ErrorMessageResourceName = nameof(DataAnnotationResource.StringLengthBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string PostalCode { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "City")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "Directions, hints")]
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// <see langword="true"/>, if <see cref="Longitude"/> and <see cref="Latitude"/> fields will be shown, else <see langword="false"/>.
    /// </summary>
    [HiddenInput]
    [Required]
    public bool ShowLatLng { get; set; }

    [Display(Name = "Longitude")]
    // if null, the range will not be checked
    [Range(-180.0, 180.0, ErrorMessageResourceName = nameof(DataAnnotationResource.ValueBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public double? Longitude { get; set; }

    [Display(Name = "Latitude")]
    // if null, the range will not be checked
    [Range(-90.0, 90.0, ErrorMessageResourceName = nameof(DataAnnotationResource.ValueBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the prefix for name (e.g. "prefix.field") and id (e.g. "prefix_field") of input fields.
    /// </summary>
    public string HtmlFieldPrefix { get; set; } = string.Empty;

    public void MapEntityToFormFields(VenueEntity venueEntity)
    {
        Id = venueEntity.Id;
        IsNew = venueEntity.IsNew;
        Name = venueEntity.Name;
        Extension = venueEntity.Extension;
        Direction = venueEntity.Direction;
        PostalCode = venueEntity.PostalCode;
        City = venueEntity.City;
        Street = venueEntity.Street;
        Latitude = venueEntity.Latitude;
        Longitude = venueEntity.Longitude;
        HtmlFieldPrefix = nameof(VenueEditModel.Venue);
    }

    public void MapFormFieldsToEntity(VenueEntity venueEntity)
    {
        // The entity is populated from the database,
        // so we can track all eventual changes to fields
        venueEntity.Id = IsNew ? default : Id ?? default;
        venueEntity.IsNew = IsNew;
        venueEntity.Name = Name;
        venueEntity.Extension = Extension;
        venueEntity.PostalCode = PostalCode;
        venueEntity.City = City;
        venueEntity.Street = Street;
        venueEntity.Direction = Direction;
        if (venueEntity is { Longitude: null, Latitude: null })
        {
            venueEntity.Longitude = Longitude;
            venueEntity.Latitude = Latitude;
        }
    }
}
