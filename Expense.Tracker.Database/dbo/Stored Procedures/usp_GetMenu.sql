CREATE Procedure [dbo].[usp_GetMenu]
(
	@pRoleId uniqueidentifier,
	@pUserId uniqueidentifier
)
AS
BEGIN

	Declare @vRoleName nvarchar(50)
	
	SELECT @vRoleName = RoleName from Role where RoleId = @pRoleId
		print @vRoleName
	IF @vRoleName = 'SuperAdmin'
	BEGIN

		SELECT * from Feature where IsListing = 1 order by ordering
	END
	ELSE
	BEGIN
		SELECT Feature.* FROM Feature
		Inner Join RoleFeature on RoleFeature.FeatureId = Feature.FeatureId And IsNull(Feature.Active, 1) = 1
		Where RoleFeature.RoleId = @pRoleId  And Feature.IsListing = 1
		order by Feature.ordering
	END
END